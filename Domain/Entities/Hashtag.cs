using Pinkterest.Domain.Common;

namespace Pinkterest.Domain.Entities;

public class Hashtag : Entity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<PhotoHashtag> PhotoHashtags { get; set; } = [];
}
