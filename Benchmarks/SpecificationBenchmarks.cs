using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using Pinkterest.Application.Common.Specifications;
using Pinkterest.Application.Photos.Search;
using Pinkterest.Domain.Entities;

namespace Pinkterest.Benchmarks;

[MemoryDiagnoser]
public class SpecificationBenchmarks
{
    private Photo _photo = default!;
    private Specification<Photo> _specification = default!;
    private Func<Photo, bool> _compiledOnce = default!;

    [GlobalSetup]
    public void Setup()
    {
        _photo = new Photo
        {
            SizeBytes = 500_000,
            UploadedUtc = DateTimeOffset.UtcNow,
            Owner = new ApplicationUser { DisplayName = "anna" }
        };

        _photo.PhotoHashtags.Add(new PhotoHashtag
        {
            Photo = _photo,
            Hashtag = new Hashtag { Name = "sunset" }
        });

        _specification = BuildSpecification();
        _compiledOnce = _specification.ToExpression().Compile();
    }

    private static Specification<Photo> BuildSpecification() =>
        new PhotoSearchBuilder()
            .WithHashtag("sunset")
            .WithAuthor("anna")
            .WithSizeBetween(1_000, 1_000_000)
            .Build();

    [Benchmark(Baseline = true, Description = "Hand-written predicate")]
    public bool HandWrittenPredicate() =>
        _photo.Owner.DisplayName.ToLower().Contains("anna")
        && _photo.PhotoHashtags.Any(link => link.Hashtag.Name == "sunset")
        && _photo.SizeBytes >= 1_000
        && _photo.SizeBytes <= 1_000_000;

    [Benchmark(Description = "Specification compiled once, then invoked")]
    public bool CompiledOnce() => _compiledOnce(_photo);

    [Benchmark(Description = "Specification recompiled on every call")]
    public bool CompiledEveryCall() => _specification.IsSatisfiedBy(_photo);

    [Benchmark(Description = "Compose four filters into one expression tree")]
    public Expression<Func<Photo, bool>> ComposeExpressionTree() => BuildSpecification().ToExpression();
}
