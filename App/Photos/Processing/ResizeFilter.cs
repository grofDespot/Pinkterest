using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Pinkterest.Application.Photos.Processing;

public sealed class ResizeFilter(int maxWidth, int maxHeight) : IImageFilter
{
    public string Name => "resize";

    public void Apply(Image image) =>
        image.Mutate(context => context.Resize(new ResizeOptions
        {
            Size = new Size(maxWidth, maxHeight),
            Mode = ResizeMode.Max
        }));
}
