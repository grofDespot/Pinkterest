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
}
