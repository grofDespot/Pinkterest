using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Pinkterest.Application.Photos.Presets;
using Pinkterest.Application.Photos.Processing;
using Pinkterest.Infrastructure.Photos.Presets;
using Xunit;

namespace Pinkterest.IntegrationTests;

public class PresetPackageFormatTests
{
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("unit-test-preset-signing-key-0123456789abcdef");
    private static readonly PresetPackageFormat Format = new(Key);

    private static readonly FilterPresetDefinition Sample =
        new(ImageOutputFormat.Jpeg, 800, 600, ["grayscale"]);

    [Fact]
    public void A_written_package_round_trips_back_to_the_same_definition()
    {
        var package = Format.Write(Sample);

        var read = Format.Read(package);

        read.IsSuccess.Should().BeTrue();
        read.Value.Format.Should().Be(ImageOutputFormat.Jpeg);
        read.Value.MaxWidth.Should().Be(800);
        read.Value.MaxHeight.Should().Be(600);
        read.Value.Filters.Should().ContainSingle().Which.Should().Be("grayscale");
    }

    [Fact]
    public void A_file_that_is_too_short_is_rejected()
    {
        var read = Format.Read(new byte[8]);

        read.IsFailure.Should().BeTrue();
        read.Error.Should().Be(FilterPresetErrors.BadFormat);
    }

    [Fact]
    public void A_file_without_the_magic_header_is_rejected()
    {
        var package = Format.Write(Sample);
        package[0] ^= 0xFF;

        Format.Read(package).Error.Should().Be(FilterPresetErrors.BadFormat);
    }

    [Fact]
    public void An_unknown_version_is_rejected()
    {
        var package = Format.Write(Sample);
        package[8] = 99;

        Format.Read(package).Error.Should().Be(FilterPresetErrors.UnsupportedVersion);
    }

    [Fact]
    public void A_declared_length_that_does_not_match_the_file_is_rejected()
    {
        var package = Format.Write(Sample);
        BinaryPrimitives.WriteInt32LittleEndian(package.AsSpan(9, 4), 12345);

        Format.Read(package).Error.Should().Be(FilterPresetErrors.BadFormat);
    }

    [Fact]
    public void A_tampered_payload_fails_the_integrity_check()
    {
        var package = Format.Write(Sample);
        package[^1] ^= 0xFF;

        Format.Read(package).Error.Should().Be(FilterPresetErrors.Tampered);
    }

    [Fact]
    public void A_correctly_signed_but_foreign_payload_is_refused_at_deserialization()
    {
        var payload = Encoding.UTF8.GetBytes(
            """{"$type":"System.IO.FileInfo, System.Private.CoreLib","Format":0,"MaxWidth":null,"MaxHeight":null,"Filters":[]}""");

        Format.Read(SignedPackage(payload)).Error.Should().Be(FilterPresetErrors.MalformedPayload);
    }

    [Fact]
    public void A_correctly_signed_but_non_json_payload_is_refused()
    {
        Format.Read(SignedPackage(Encoding.UTF8.GetBytes("not json at all"))).Error
            .Should().Be(FilterPresetErrors.MalformedPayload);
    }

    private static byte[] SignedPackage(byte[] payload)
    {
        var magic = "PKPRESET"u8.ToArray();
        var header = new byte[13];
        magic.CopyTo(header, 0);
        header[8] = 1;
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(9, 4), payload.Length);

        var signed = header.Concat(payload).ToArray();
        var mac = HMACSHA256.HashData(Key, signed);

        return signed.Concat(mac).ToArray();
    }
}
