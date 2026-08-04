using Pinkterest.Domain.Common;

namespace Pinkterest.Domain.Entities;

public class Photo : Entity
{
    public Guid OwnerId { get; set; }

    public ApplicationUser Owner { get; set; } = null!;

    public string Description { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string StorageKey { get; set; } = string.Empty;

    public string? ThumbnailKey { get; set; }

    public string ContentType { get; set; } = string.Empty;

    public int Width { get; set; }

    public int Height { get; set; }

    public long SizeBytes { get; set; }

    public DateTimeOffset UploadedUtc { get; set; }

    public bool IsDeleted { get; set; }

    public ICollection<PhotoHashtag> PhotoHashtags { get; set; } = [];
}
