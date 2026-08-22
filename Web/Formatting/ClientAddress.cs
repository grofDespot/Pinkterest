using System.Net;
using System.Net.Sockets;

namespace Pinkterest.Web.Formatting;

public static class ClientAddress
{
    public static string ForDisplay(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return "—";
        }

        return IPAddress.TryParse(address, out var parsed) && IsInternal(parsed)
            ? "internal"
            : address;
    }

    private static bool IsInternal(IPAddress address)
    {
        if (IPAddress.IsLoopback(address))
        {
            return true;
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            return address.IsIPv6LinkLocal
                || address.IsIPv6SiteLocal
                || address.IsIPv6UniqueLocal
                || (address.IsIPv4MappedToIPv6 && IsInternal(address.MapToIPv4()));
        }

        var octets = address.GetAddressBytes();

        return octets[0] switch
        {
            10 => true,
            127 => true,
            169 => octets[1] == 254,
            172 => octets[1] >= 16 && octets[1] <= 31,
            192 => octets[1] == 168,
            _ => false
        };
    }
}
