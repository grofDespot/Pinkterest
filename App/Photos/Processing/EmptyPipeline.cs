using SixLabors.ImageSharp;

namespace Pinkterest.Application.Photos.Processing;

public sealed class EmptyPipeline : IImagePipeline
{
    public static readonly IImagePipeline Instance = new EmptyPipeline();

    private EmptyPipeline()
    {
    }

    public void Apply(Image image)
    {
    }
}
