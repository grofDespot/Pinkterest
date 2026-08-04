using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Pinkterest.Application.Photos.Processing;

public sealed class SepiaFilter : IImageFilter
{
    public string Name => "sepia";

    public void Apply(Image image) => image.Mutate(context => context.Sepia());
}
