using SixLabors.ImageSharp;

namespace Pinkterest.Application.Photos.Processing;

public sealed class FilterPipelineDecorator(IImagePipeline inner, IImageFilter filter) : IImagePipeline
{
    public void Apply(Image image)
    {
        inner.Apply(image);
        filter.Apply(image);
    }
}
