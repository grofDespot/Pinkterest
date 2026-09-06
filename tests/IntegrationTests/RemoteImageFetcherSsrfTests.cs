using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Pinkterest.Application.Photos.Import;
using Pinkterest.Infrastructure.Photos.Import;
using Xunit;

namespace Pinkterest.IntegrationTests;

public class RemoteImageFetcherSsrfTests
{
    private static RemoteImageFetcher Fetcher() =>
        new(new HttpClient(), NullLogger<RemoteImageFetcher>.Instance);

    [Fact]
    public async Task A_value_that_is_not_an_absolute_url_is_rejected()
    {
        var result = await Fetcher().FetchAsync("not a url");

        result.Error.Should().Be(ImageImportErrors.InvalidUrl);
    }

    [Fact]
    public async Task A_non_https_scheme_is_rejected()
    {
        var result = await Fetcher().FetchAsync("http://example.com/cat.png");

        result.Error.Should().Be(ImageImportErrors.SchemeNotAllowed);
    }

    [Theory]
    [InlineData("https://127.0.0.1/x.png")]
    [InlineData("https://10.0.0.1/x.png")]
    [InlineData("https://192.168.1.1/x.png")]
    [InlineData("https://[::1]/x.png")]
    public async Task A_host_that_resolves_to_a_private_or_loopback_address_is_blocked(string url)
    {
        var result = await Fetcher().FetchAsync(url);

        result.Error.Should().Be(ImageImportErrors.BlockedAddress);
    }
}
