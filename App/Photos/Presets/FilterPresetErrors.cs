using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Photos.Presets;

public static class FilterPresetErrors
{
    public static readonly Error NameRequired =
        new("Preset.NameRequired", "Give the preset a name.");

    public static readonly Error NameTaken =
        new("Preset.NameTaken", "You already have a preset with that name.");

    public static readonly Error UnknownFilter =
        new("Preset.UnknownFilter", "That preset refers to a filter this application does not provide.");

    public static Error LimitReached(int limit) =>
        new("Preset.LimitReached", $"You can keep at most {limit} presets.");

    public static readonly Error BadFormat =
        new("Preset.BadFormat", "The file is not a Pinkterest preset package.");

    public static readonly Error UnsupportedVersion =
        new("Preset.UnsupportedVersion", "The preset package version is not supported.");

    public static readonly Error Tampered =
        new("Preset.Tampered", "The preset package failed its integrity check.");

    public static readonly Error MalformedPayload =
        new("Preset.MalformedPayload", "The preset package contents could not be read.");

    public static readonly Error NotFound =
        new("Preset.NotFound", "The preset was not found.");
}
