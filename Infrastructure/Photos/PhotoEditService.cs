using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pinkterest.Application.Common.Events;
using Pinkterest.Application.Common.Results;
using Pinkterest.Application.Photos;
using Pinkterest.Domain.Entities;
using Pinkterest.Domain.Events;
using Pinkterest.Infrastructure.Persistence;

using Pinkterest.Application.Common.Auditing;

using Pinkterest.CrossCutting.Auditing;

namespace Pinkterest.Infrastructure.Photos;

public sealed class PhotoEditService(
    ApplicationDbContext context,
    IPhotoRepository repository,
    IDomainEventDispatcher dispatcher,
    TimeProvider timeProvider,
    ILogger<PhotoEditService> logger) : IPhotoEditService
{
    [Audited(AuditActions.PhotoEdit, EntityType = nameof(Photo))]
    public async Task<Result> UpdateDetailsAsync(
        Guid photoId,
        Guid editorId,
        bool editorIsAdministrator,
        string description,
        IReadOnlyList<string> hashtags,
        CancellationToken cancellationToken = default)
    {
        var photo = await repository.GetForUpdateAsync(photoId, cancellationToken);

        if (photo is null)
        {
            return Result.Failure(Error.NotFound("Photo"));
        }

        if (photo.OwnerId != editorId && !editorIsAdministrator)
        {
            logger.LogWarning("User {EditorId} attempted to edit photo {PhotoId} they do not own.", editorId, photoId);
            return Result.Failure(Error.Forbidden("edit this photo"));
        }

        photo.Description = description;
        await ReplaceHashtagsAsync(photo, hashtags, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await dispatcher.PublishAsync(
            new PhotoDetailsUpdatedEvent(photo.Id, editorId, editorIsAdministrator, timeProvider.GetUtcNow()),
            cancellationToken);

        return Result.Success();
    }

    private async Task ReplaceHashtagsAsync(
        Photo photo,
        IReadOnlyList<string> requested,
        CancellationToken cancellationToken)
    {
        var names = HashtagNormalizer.Normalize(requested);

        photo.PhotoHashtags.Clear();

        if (names.Count == 0)
        {
            return;
        }

        var existing = await context.Hashtags
            .Where(hashtag => names.Contains(hashtag.Name))
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
}
