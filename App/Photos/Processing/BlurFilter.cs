using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Pinkterest.Application.Photos.Processing;

public sealed class BlurFilter(float sigma) : IImageFilter
{
    public string Name => "blur";

    public void Apply(Image image) => image.Mutate(context => context.GaussianBlur(sigma));
}
