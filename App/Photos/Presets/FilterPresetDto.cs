using Pinkterest.Application.Photos.Processing;

namespace Pinkterest.Application.Photos.Presets;

public sealed record FilterPresetDto(
    Guid Id,
    string Name,
    ImageProcessingOptions Options,
    DateTimeOffset CreatedUtc)
{
    public string Summary
    {
        get
        {
            var parts = new List<string> { Options.Format.ToString() };

            if (Options.MaxWidth is > 0 || Options.MaxHeight is > 0)
            {
                parts.Add($"{Options.MaxWidth?.ToString() ?? "auto"}x{Options.MaxHeight?.ToString() ?? "auto"}");
            }

            parts.AddRange(Options.Filters);
            return string.Join(" · ", parts);
        }
    }
}
