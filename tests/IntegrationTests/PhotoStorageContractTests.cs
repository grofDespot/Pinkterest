using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Pinkterest.Application.Photos.Storage;
using Pinkterest.Infrastructure.Storage;
using Xunit;

namespace Pinkterest.IntegrationTests;

/// Every implementation of IPhotoStorage must satisfy exactly these assertions.
/// Substitutability is demonstrated by the suite passing unchanged for each one.
public abstract class PhotoStorageContractTests : IDisposable
{
    private const string ThumbnailKey = "thumbnails/contract/thumb.jpg";
    private const string PhotoKey = "photos/contract/original.jpg";

    private readonly string _root = Path.Combine(
        Path.GetTempPath(), "pinkterest-contract", Guid.CreateVersion7().ToString("N"));

    protected PhotoStorageContractTests() => Directory.CreateDirectory(_root);

    protected abstract IPhotoStorage CreateStorage(string root);

    private IPhotoStorage Storage => CreateStorage(_root);

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        try
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
        catch (IOException)
        {
        }
    }

    private static Stream Content(string text) => new MemoryStream(Encoding.UTF8.GetBytes(text));

    private static async Task<string> ReadAsync(IPhotoStorage storage, string key)
    {
        await using var stream = await storage.OpenReadAsync(key);
        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync();
    }

    [Theory]
    [InlineData(ThumbnailKey)]
    [InlineData(PhotoKey)]
    public async Task What_was_saved_is_what_is_read_back(string key)
    {
        var storage = Storage;

        await storage.SaveAsync(key, Content("the original bytes"), "image/jpeg");

        (await ReadAsync(storage, key)).Should().Be("the original bytes");
    }

    [Theory]
    [InlineData(ThumbnailKey)]
    [InlineData(PhotoKey)]
    public async Task Exists_reports_presence_and_absence(string key)
    {
        var storage = Storage;

        (await storage.ExistsAsync(key)).Should().BeFalse();

        await storage.SaveAsync(key, Content("something"), "image/jpeg");

        (await storage.ExistsAsync(key)).Should().BeTrue();
    }

    [Theory]
    [InlineData(ThumbnailKey)]
    [InlineData(PhotoKey)]
    public async Task Overwriting_replaces_the_previous_content(string key)
    {
        var storage = Storage;

        await storage.SaveAsync(key, Content("first"), "image/jpeg");
        await ReadAsync(storage, key);
        await storage.SaveAsync(key, Content("second"), "image/jpeg");

        (await ReadAsync(storage, key)).Should().Be(
            "second",
            "a read after a write must never return stale content, whatever caching sits in between");
    }

    [Theory]
    [InlineData(ThumbnailKey)]
    [InlineData(PhotoKey)]
    public async Task Deleting_removes_the_object(string key)
    {
        var storage = Storage;

        await storage.SaveAsync(key, Content("doomed"), "image/jpeg");
        await ReadAsync(storage, key);
        await storage.DeleteAsync(key);

        (await storage.ExistsAsync(key)).Should().BeFalse();
    }

    [Fact]
    public async Task Deleting_something_that_is_not_there_is_not_an_error()
    {
        var storage = Storage;

        var act = async () => await storage.DeleteAsync("photos/never/existed.jpg");

        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData("../escaped.jpg")]
    [InlineData("photos/../../escaped.jpg")]
    [InlineData("photos/../../../../windows/system32/config.dat")]
    public async Task Keys_that_resolve_outside_the_root_are_refused(string key)
    {
        var storage = Storage;

        var act = async () => await storage.SaveAsync(key, Content("payload"), "image/jpeg");

        await act.Should().ThrowAsync<Exception>(
            "a storage key is untrusted input and must never reach a path above the root");
    }

    [Fact]
    public async Task A_leading_slash_is_treated_as_root_relative_rather_than_absolute()
    {
        var storage = Storage;

        await storage.SaveAsync("/photos/rooted.jpg", Content("contained"), "image/jpeg");

        (await ReadAsync(storage, "photos/rooted.jpg")).Should().Be(
            "contained",
            "a leading slash is normalised away, so the key stays inside the storage root");
    }

    [Fact]
    public void The_provider_names_itself()
    {
        Storage.ProviderName.Should().NotBeNullOrWhiteSpace();
    }

    private sealed class ContractHostEnvironment(string root) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Pinkterest.ContractTests";
        public string ContentRootPath { get; set; } = root;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    protected static LocalFileSystemPhotoStorage LocalStorage(string root) =>
        new(Options.Create(new StorageOptions
            {
                Provider = StorageProvider.Local,
                LocalRootPath = root
            }),
            new ContractHostEnvironment(root));
}

public sealed class LocalFileSystemPhotoStorageContractTests : PhotoStorageContractTests
{
    protected override IPhotoStorage CreateStorage(string root) => LocalStorage(root);
}

public sealed class CachingPhotoStorageProxyContractTests : PhotoStorageContractTests
{
    private readonly IMemoryCache _cache =
        new MemoryCache(new MemoryCacheOptions { SizeLimit = 8L * 1024 * 1024 });

    protected override IPhotoStorage CreateStorage(string root) =>
        new CachingPhotoStorageProxy(LocalStorage(root), _cache);
}
