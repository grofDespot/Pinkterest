using FluentAssertions;
using Pinkterest.Application.Photos.Search;
using Pinkterest.Domain.Entities;
using Xunit;

namespace Pinkterest.UnitTests.Photos;

public class PhotoSearchBuilderTests
{
    private static Photo Photo(string author, long sizeBytes, DateTimeOffset uploaded, params string[] hashtags)
    {
        var photo = new Photo
        {
            SizeBytes = sizeBytes,
            UploadedUtc = uploaded,
            Owner = new ApplicationUser { DisplayName = author }
        };

        foreach (var hashtag in hashtags)
        {
            photo.PhotoHashtags.Add(new PhotoHashtag { Photo = photo, Hashtag = new Hashtag { Name = hashtag } });
        }

        return photo;
    }

    [Fact]
    public void An_empty_search_matches_everything()
    {
        var specification = new PhotoSearchBuilder().Build();

        specification.IsSatisfiedBy(Photo("anna", 10, DateTimeOffset.UtcNow)).Should().BeTrue();
    }

    [Fact]
    public void Blank_inputs_are_ignored_rather_than_narrowing_the_result()
    {
        var specification = new PhotoSearchBuilder()
            .WithHashtag("   ")
            .WithAuthor(null)
            .WithSizeBetween(null, null)
            .UploadedBetween(null, null)
            .Build();

        specification.IsSatisfiedBy(Photo("anna", 10, DateTimeOffset.UtcNow)).Should().BeTrue();
    }

    [Fact]
    public void Supplied_filters_are_combined_with_and()
    {
        var specification = new PhotoSearchBuilder()
            .WithHashtag("#Sunset")
            .WithAuthor("anna")
            .WithSizeBetween(100, 1000)
            .Build();

        specification.IsSatisfiedBy(Photo("anna", 500, DateTimeOffset.UtcNow, "sunset")).Should().BeTrue();
        specification.IsSatisfiedBy(Photo("boris", 500, DateTimeOffset.UtcNow, "sunset")).Should().BeFalse();
        specification.IsSatisfiedBy(Photo("anna", 5000, DateTimeOffset.UtcNow, "sunset")).Should().BeFalse();
        specification.IsSatisfiedBy(Photo("anna", 500, DateTimeOffset.UtcNow, "beach")).Should().BeFalse();
    }

    [Fact]
    public void A_date_range_covers_the_whole_of_the_final_day()
    {
        var day = new DateOnly(2026, 3, 14);

        var specification = new PhotoSearchBuilder().UploadedBetween(day, day).Build();

        var lateThatEvening = new DateTimeOffset(2026, 3, 14, 23, 45, 0, TimeSpan.Zero);
        specification.IsSatisfiedBy(Photo("anna", 10, lateThatEvening)).Should().BeTrue();

        var nextMorning = new DateTimeOffset(2026, 3, 15, 0, 30, 0, TimeSpan.Zero);
        specification.IsSatisfiedBy(Photo("anna", 10, nextMorning)).Should().BeFalse();
    }
}
