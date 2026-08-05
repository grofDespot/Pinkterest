using FluentAssertions;
using Pinkterest.Application.Photos;
using Xunit;

namespace Pinkterest.UnitTests.Photos;

public class HashtagNormalizerTests
{
    [Fact]
    public void Leading_hashes_are_removed_and_case_is_folded()
    {
        HashtagNormalizer.Parse("#Sunset").Should().ContainSingle().Which.Should().Be("sunset");
    }

    [Fact]
    public void Commas_and_spaces_both_separate_tags()
    {
        HashtagNormalizer.Parse("sunset, beach summer")
            .Should().BeEquivalentTo("sunset", "beach", "summer");
    }

    [Fact]
    public void Duplicates_are_collapsed()
    {
        HashtagNormalizer.Parse("sunset #Sunset SUNSET").Should().ContainSingle();
    }

    [Fact]
    public void Punctuation_is_stripped_but_hyphens_and_underscores_survive()
    {
        HashtagNormalizer.Parse("gold-hour! my_photo?")
            .Should().BeEquivalentTo("gold-hour", "my_photo");
    }

    [Fact]
    public void Empty_input_produces_no_tags()
    {
        HashtagNormalizer.Parse(null).Should().BeEmpty();
        HashtagNormalizer.Parse("   ").Should().BeEmpty();
    }
}
