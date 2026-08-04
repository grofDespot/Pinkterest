using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pinkterest.Application.Common.Results;
using Pinkterest.Application.Photos;
using Pinkterest.Application.Photos.Processing;
using Pinkterest.Application.Photos.Storage;
using Pinkterest.Application.Photos.Validation;
using Pinkterest.Domain.Entities;
using Pinkterest.Infrastructure.Persistence;

namespace Pinkterest.Infrastructure.Photos;

public sealed class PhotoUploadService(
    ApplicationDbContext context,
    IPhotoStorage storage,
    IImageProcessor imageProcessor,
    TimeProvider timeProvider,
    ILogger<PhotoUploadService> logger) : IPhotoUploadService
{
    private const int ThumbnailMaxEdge = 400;

    public async Task<Result<Guid>> UploadAsync(
        UploadPhotoRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .Include(u => u.Package)
            .SingleOrDefaultAsync(u => u.Id == request.OwnerId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<Guid>(Error.NotFound("User"));
        }

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        var daily = await context.DailyUsages
            .SingleOrDefaultAsync(d => d.UserId == user.Id && d.Date == today, cancellationToken);

        var totalBytesStored = await context.Photos
            .Where(p => p.OwnerId == user.Id)
            .SumAsync(p => (long?)p.SizeBytes, cancellationToken) ?? 0L;

        var validation = UploadValidationChain.Build().Handle(new UploadValidationContext(
            request.FileName,
            request.ContentType,
            request.SizeBytes,
            user.Package.MaxUploadSizeBytes,
            user.Package.DailyUploadLimit,
            daily?.UploadCount ?? 0,
            user.Package.MaxTotalStorageBytes,
            totalBytesStored));

        if (validation.IsFailure)
        {
            return Result.Failure<Guid>(validation.Error);
        }

        ProcessedImage processed;
        ProcessedImage thumbnail;

        try
        {
            request.Content.Position = 0;
            processed = await imageProcessor.ProcessAsync(request.Content, request.Processing, cancellationToken);

            request.Content.Position = 0;
            thumbnail = await imageProcessor.ProcessAsync(
                request.Content,
                new ImageProcessingOptions(ImageOutputFormat.Jpeg, ThumbnailMaxEdge, ThumbnailMaxEdge, []),
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Rejected an upload that could not be decoded as an image.");
            return Result.Failure<Guid>(PhotoErrors.NotAnImage);
        }

        await using (processed)
        await using (thumbnail)
        {
            var photo = new Photo
            {
                OwnerId = user.Id,
                Description = request.Description,
                OriginalFileName = Path.GetFileName(request.FileName),
                ContentType = processed.ContentType,
                Width = processed.Width,
                Height = processed.Height,
                SizeBytes = processed.SizeBytes,
                UploadedUtc = timeProvider.GetUtcNow()
            };

            photo.StorageKey = StorageKeys.ForPhoto(user.Id, photo.Id, processed.FileExtension);
            photo.ThumbnailKey = StorageKeys.ForThumbnail(user.Id, photo.Id);

            await storage.SaveAsync(photo.StorageKey, processed.Content, processed.ContentType, cancellationToken);
            await storage.SaveAsync(photo.ThumbnailKey, thumbnail.Content, thumbnail.ContentType, cancellationToken);

            await AttachHashtagsAsync(photo, request.Hashtags, cancellationToken);

            context.Photos.Add(photo);
            TrackUsage(daily, user.Id, today, photo.SizeBytes);

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("User {UserId} uploaded photo {PhotoId}.", user.Id, photo.Id);
            return Result.Success(photo.Id);
        }
    }

    private async Task AttachHashtagsAsync(
        Photo photo,
        IReadOnlyList<string> requested,
        CancellationToken cancellationToken)
    {
        var names = HashtagNormalizer.Normalize(requested);

        if (names.Count == 0)
        {
            return;
        }

        var existing = await context.Hashtags
            .Where(h => names.Contains(h.Name))
            .ToListAsync(cancellationToken);

        foreach (var name in names)
        {
            var hashtag = existing.SingleOrDefault(h => h.Name == name);

            if (hashtag is null)
            {
                hashtag = new Hashtag { Name = name };
                context.Hashtags.Add(hashtag);
            }

            photo.PhotoHashtags.Add(new PhotoHashtag { Photo = photo, Hashtag = hashtag });
        }
    }

    private void TrackUsage(DailyUsage? daily, Guid userId, DateOnly today, long sizeBytes)
    {
        if (daily is null)
        {
            context.DailyUsages.Add(new DailyUsage
            {
                UserId = userId,
                Date = today,
                UploadCount = 1,
                BytesUploaded = sizeBytes
            });

            return;
        }

        daily.UploadCount++;
        daily.BytesUploaded += sizeBytes;
    }
}
