namespace Pinkterest.Application.Photos.Processing;

public static class ImagePipeline
{
    public static IImagePipeline Compose(IEnumerable<IImageFilter> filters) =>
        filters.Aggregate(
            EmptyPipeline.Instance,
            (inner, filter) => new FilterPipelineDecorator(inner, filter));
}
