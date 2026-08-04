using Microsoft.EntityFrameworkCore;
using Pinkterest.Application.Common.Specifications;
using Pinkterest.Application.Photos;
using Pinkterest.Application.Photos.Queries;
using Pinkterest.Domain.Entities;
using Pinkterest.Infrastructure.Persistence;

namespace Pinkterest.Infrastructure.Photos;

public sealed class PhotoRepository(ApplicationDbContext context) : IPhotoRepository
{
    public async Task<IReadOnlyList<PhotoSummary>> ListAsync(
        Specification<Photo> specification,
        int take,
        int skip = 0,
        CancellationToken cancellationToken = default) =>
        await context.Photos
            .AsNoTracking()
            .Where(specification.ToExpression())
            .OrderByDescending(photo => photo.UploadedUtc)
            .Skip(skip)
            .Take(take)
            .Select(photo => new PhotoSummary(
                photo.Id,
                photo.Description,
                photo.Owner.DisplayName,
                photo.OwnerId,
                photo.UploadedUtc,
                photo.Width,
                photo.Height,
                photo.SizeBytes,
                photo.PhotoHashtags.Select(link => link.Hashtag.Name).ToList()))
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(
        Specification<Photo> specification,
        CancellationToken cancellationToken = default) =>
        context.Photos
            .AsNoTracking()
            .Where(specification.ToExpression())
            .CountAsync(cancellationToken);

    public Task<PhotoDetail?> GetDetailAsync(Guid photoId, CancellationToken cancellationToken = default) =>
        context.Photos
            .AsNoTracking()
            .Where(photo => photo.Id == photoId)
            .Select(photo => new PhotoDetail(
                photo.Id,
                photo.Description,
                photo.OriginalFileName,
                photo.ContentType,
                photo.Owner.DisplayName,
                photo.OwnerId,
                photo.UploadedUtc,
                photo.Width,
                photo.Height,
                photo.SizeBytes,
                photo.PhotoHashtags.Select(link => link.Hashtag.Name).ToList()))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<Photo?> GetForUpdateAsync(Guid photoId, CancellationToken cancellationToken = default) =>
        context.Photos
            .Include(photo => photo.PhotoHashtags)
            .SingleOrDefaultAsync(photo => photo.Id == photoId, cancellationToken);

    public async Task<(string StorageKey, string ContentType, Guid OwnerId)?> GetStorageInfoAsync(
        Guid photoId,
        bool thumbnail,
        CancellationToken cancellationToken = default)
    {
        var record = await context.Photos
            .AsNoTracking()
            .Where(photo => photo.Id == photoId)
            .Select(photo => new
            {
                photo.StorageKey,
                photo.ThumbnailKey,
                photo.ContentType,
                photo.OwnerId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (record is null)
        {
            return null;
        }

        return thumbnail
            ? record.ThumbnailKey is null
                ? null
                : (record.ThumbnailKey, "image/jpeg", record.OwnerId)
            : (record.StorageKey, record.ContentType, record.OwnerId);
    }
}
