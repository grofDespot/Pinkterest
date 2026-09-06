using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pinkterest.Application.Common.Results;
using Pinkterest.Application.Photos.Presets;

namespace Pinkterest.Infrastructure.Photos.Presets;

public sealed class PresetPackageFormat
{
    private static readonly byte[] Magic = "PKPRESET"u8.ToArray();
    private const byte Version = 1;
    private const int MaxPayloadBytes = 64 * 1024;

    private const int MagicLength = 8;
    private const int HeaderLength = MagicLength + 1 + 4;   // magic + version + payload length
    private const int HmacLength = 32;

    private readonly byte[] _key;

    // The deserialization target is fixed to FilterPresetDefinition, so no type named
    // in the payload can be instantiated — there is no polymorphic type handling to abuse.
    // UnmappedMemberHandling.Disallow rejects any field outside that shape, including a
    // smuggled $type discriminator, and MaxDepth bounds nesting.
    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        MaxDepth = 8,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public PresetPackageFormat(byte[] key) => _key = key;

    public byte[] Write(FilterPresetDefinition definition)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(definition);

        var package = new byte[HeaderLength + payload.Length + HmacLength];
        Magic.CopyTo(package, 0);
        package[MagicLength] = Version;
        BinaryPrimitives.WriteInt32LittleEndian(package.AsSpan(MagicLength + 1, 4), payload.Length);
        payload.CopyTo(package, HeaderLength);

        var mac = HMACSHA256.HashData(_key, package.AsSpan(0, HeaderLength + payload.Length));
        mac.CopyTo(package, HeaderLength + payload.Length);

        return package;
    }

    public Result<FilterPresetDefinition> Read(byte[] package)
    {
        // 1. Structural check: is this even our format?
        if (package.Length < HeaderLength + HmacLength)
        {
            return Result.Failure<FilterPresetDefinition>(FilterPresetErrors.BadFormat);
        }

        if (!package.AsSpan(0, MagicLength).SequenceEqual(Magic))
        {
            return Result.Failure<FilterPresetDefinition>(FilterPresetErrors.BadFormat);
        }

        // 2. Known version only.
        if (package[MagicLength] != Version)
        {
            return Result.Failure<FilterPresetDefinition>(FilterPresetErrors.UnsupportedVersion);
        }

        // 3. Declared length checked against the actual stream before trusting it.
        var declared = BinaryPrimitives.ReadInt32LittleEndian(package.AsSpan(MagicLength + 1, 4));

        if (declared < 0 || declared > MaxPayloadBytes || package.Length != HeaderLength + declared + HmacLength)
        {
            return Result.Failure<FilterPresetDefinition>(FilterPresetErrors.BadFormat);
        }

        // 4. Integrity: verify the HMAC before anything parses the payload.
        var signed = package.AsSpan(0, HeaderLength + declared);
        var expected = package.AsSpan(HeaderLength + declared, HmacLength);
        var actual = HMACSHA256.HashData(_key, signed);

        if (!CryptographicOperations.FixedTimeEquals(actual, expected))
        {
            return Result.Failure<FilterPresetDefinition>(FilterPresetErrors.Tampered);
        }

        // 5. Whitelisted deserialization: only FilterPresetDefinition may be built.
        try
        {
            var payload = package.AsSpan(HeaderLength, declared);
            var definition = JsonSerializer.Deserialize<FilterPresetDefinition>(payload, ReadOptions);

            return definition is null
                ? Result.Failure<FilterPresetDefinition>(FilterPresetErrors.MalformedPayload)
                : Result.Success(definition);
        }
        catch (JsonException)
        {
            return Result.Failure<FilterPresetDefinition>(FilterPresetErrors.MalformedPayload);
        }
    }
}
