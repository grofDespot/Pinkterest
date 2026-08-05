using FluentAssertions;
using Pinkterest.Application.Common.Specifications;
using Pinkterest.Application.Photos.Specifications;
using Pinkterest.Domain.Entities;
using Xunit;

namespace Pinkterest.UnitTests.Common;

public class SpecificationTests
{
    private static Photo PhotoWith(string author, long sizeBytes, params string[] hashtags)
    {
        var photo = new Photo
        {
            SizeBytes = sizeBytes,
            Owner = new ApplicationUser { DisplayName = author }
        };

        foreach (var hashtag in hashtags)
        {
            photo.PhotoHashtags.Add(new PhotoHashtag
            {
                Photo = photo,
                Hashtag = new Hashtag { Name = hashtag }
            });
        }

        return photo;
    }

    [Fact]
    public void All_matches_everything()
    {
        Specification<Photo>.All.IsSatisfiedBy(PhotoWith("anna", 100)).Should().BeTrue();
    }

    [Fact]
    public void And_requires_both_sides()
    {
        var specification = new PhotoByAuthorNameSpecification("anna")
            .And(new PhotoSizeBetweenSpecification(50, 200));

        specification.IsSatisfiedBy(PhotoWith("anna", 100)).Should().BeTrue();
        specification.IsSatisfiedBy(PhotoWith("anna", 900)).Should().BeFalse();
        specification.IsSatisfiedBy(PhotoWith("boris", 100)).Should().BeFalse();
    }

    [Fact]
    public void Or_requires_either_side()
    {
        var specification = new PhotoByAuthorNameSpecification("anna")
            .Or(new PhotoByAuthorNameSpecification("boris"));

        specification.IsSatisfiedBy(PhotoWith("boris", 10)).Should().BeTrue();
        specification.IsSatisfiedBy(PhotoWith("clara", 10)).Should().BeFalse();
    }

    [Fact]
    public void Not_inverts_the_inner_specification()
    {
        var specification = new PhotoByAuthorNameSpecification("anna").Not();

        specification.IsSatisfiedBy(PhotoWith("anna", 10)).Should().BeFalse();
        specification.IsSatisfiedBy(PhotoWith("boris", 10)).Should().BeTrue();
    }

    [Fact]
    public void Composition_does_not_rebind_parameters_of_nested_lambdas()
    {
        var specification = new PhotoWithHashtagSpecification("sunset")
            .And(new PhotoSizeBetweenSpecification(null, 1000));

        var act = () => specification.IsSatisfiedBy(PhotoWith("anna", 100, "sunset"));

        act.Should().NotThrow(
            "the Any(link => link.Hashtag.Name == ...) lambda has its own parameter that must survive composition");

        specification.IsSatisfiedBy(PhotoWith("anna", 100, "sunset")).Should().BeTrue();
        specification.IsSatisfiedBy(PhotoWith("anna", 100, "beach")).Should().BeFalse();
    }

    [Fact]
    public void Three_specifications_compose_without_losing_any_condition()
    {
        var specification = new PhotoByAuthorNameSpecification("anna")
            .And(new PhotoWithHashtagSpecification("sunset"))
            .And(new PhotoSizeBetweenSpecification(50, 200));

        specification.IsSatisfiedBy(PhotoWith("anna", 100, "sunset")).Should().BeTrue();
        specification.IsSatisfiedBy(PhotoWith("anna", 100, "beach")).Should().BeFalse();
        specification.IsSatisfiedBy(PhotoWith("anna", 500, "sunset")).Should().BeFalse();
    }
}
