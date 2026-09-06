using FluentAssertions;
using Pinkterest.Web.Formatting;
using Xunit;

namespace Pinkterest.IntegrationTests;

public class ClientAddressTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void A_missing_address_is_shown_as_a_dash(string? address)
    {
        ClientAddress.ForDisplay(address).Should().Be("—");
    }

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("10.1.2.3")]
    [InlineData("172.16.0.1")]
    [InlineData("172.31.255.255")]
    [InlineData("192.168.1.1")]
    [InlineData("169.254.10.10")]
    [InlineData("::1")]
    public void An_internal_address_is_masked(string address)
    {
        ClientAddress.ForDisplay(address).Should().Be("internal");
    }

    [Theory]
    [InlineData("8.8.8.8")]
    [InlineData("172.15.0.1")]
    [InlineData("203.0.113.5")]
    [InlineData("not-an-ip")]
    public void A_public_or_unparsable_address_is_shown_verbatim(string address)
    {
        ClientAddress.ForDisplay(address).Should().Be(address);
    }
}
