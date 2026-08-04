namespace Pinkterest.Domain.Entities;

public class PhotoHashtag
{
    public Guid PhotoId { get; set; }

    public Photo Photo { get; set; } = null!;

    public Guid HashtagId { get; set; }

    public Hashtag Hashtag { get; set; } = null!;
}
