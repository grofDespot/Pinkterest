using System.Text.RegularExpressions;

namespace Pinkterest.Application.Photos;

public static partial class HashtagNormalizer
{
    private const int MaxLength = 64;

    public static IReadOnlyList<string> Normalize(IEnumerable<string> raw) =>
        raw.SelectMany(Split)
            .Select(Clean)
            .Where(tag => tag.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .Take(20)
            .ToList();

    public static IReadOnlyList<string> Parse(string? input) =>
        string.IsNullOrWhiteSpace(input) ? [] : Normalize([input]);

    private static IEnumerable<string> Split(string value) =>
        value.Split([',', ' ', ';', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries);

    private static string Clean(string value)
    {
        var trimmed = InvalidCharacters().Replace(value.Trim().TrimStart('#').ToLowerInvariant(), string.Empty);
        return trimmed.Length > MaxLength ? trimmed[..MaxLength] : trimmed;
    }

    [GeneratedRegex("[^a-z0-9_-]")]
    private static partial Regex InvalidCharacters();
}
