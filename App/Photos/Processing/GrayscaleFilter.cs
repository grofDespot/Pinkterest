using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Pinkterest.Application.Photos.Processing;

public sealed class GrayscaleFilter : IImageFilter
{
    public string Name => "grayscale";

    public void Apply(Image image) => image.Mutate(context => context.Grayscale());
}
