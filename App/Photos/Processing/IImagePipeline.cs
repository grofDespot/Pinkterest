using SixLabors.ImageSharp;

namespace Pinkterest.Application.Photos.Processing;

public interface IImagePipeline
{
    void Apply(Image image);
}
