using SixLabors.ImageSharp;

namespace Pinkterest.Application.Photos.Processing;

public interface IImageFilter
{
    string Name { get; }

    void Apply(Image image);
}
