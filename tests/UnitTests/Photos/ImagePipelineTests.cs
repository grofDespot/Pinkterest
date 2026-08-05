using FluentAssertions;
using Pinkterest.Application.Photos.Processing;
using SixLabors.ImageSharp;
using Xunit;

namespace Pinkterest.UnitTests.Photos;

public class ImagePipelineTests
{
    private sealed class RecordingFilter(string name, List<string> log) : IImageFilter
    {
        public string Name => name;

        public void Apply(Image image) => log.Add(name);
    }

    [Fact]
    public void An_empty_pipeline_applies_nothing()
    {
        using var image = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(4, 4);

        var act = () => ImagePipeline.Compose([]).Apply(image);

        act.Should().NotThrow();
    }

    [Fact]
    public void Filters_are_applied_in_the_order_they_were_composed()
    {
        var log = new List<string>();
        using var image = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(4, 4);

        ImagePipeline.Compose([
            new RecordingFilter("resize", log),
            new RecordingFilter("sepia", log),
            new RecordingFilter("blur", log)
        ]).Apply(image);

        log.Should().Equal("resize", "sepia", "blur");
    }

    [Fact]
    public void The_catalog_creates_only_known_filters()
    {
        ImageFilterCatalog.Create("sepia").Should().BeOfType<SepiaFilter>();
        ImageFilterCatalog.Create("grayscale").Should().BeOfType<GrayscaleFilter>();
        ImageFilterCatalog.Create("blur").Should().BeOfType<BlurFilter>();
        ImageFilterCatalog.Create("dropTables").Should().BeNull();
    }

    [Fact]
    public void Unknown_filter_names_are_discarded_rather_than_throwing()
    {
        ImageFilterCatalog.CreateMany(["sepia", "nonsense", "blur"])
            .Select(filter => filter.Name)
            .Should().Equal("sepia", "blur");
    }
}
