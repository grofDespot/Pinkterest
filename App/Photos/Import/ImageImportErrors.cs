using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Photos.Import;

public static class ImageImportErrors
{
    public static readonly Error InvalidUrl =
        new("Import.InvalidUrl", "Provide a valid absolute https URL.");

    public static readonly Error SchemeNotAllowed =
        new("Import.SchemeNotAllowed", "Only https URLs are allowed.");

    public static readonly Error BlockedAddress =
        new("Import.BlockedAddress", "The host resolves to an address that is not allowed.");

    public static readonly Error Unreachable =
        new("Import.Unreachable", "The URL could not be retrieved.");

    public static readonly Error TooLarge =
        new("Import.TooLarge", "The file exceeds the maximum import size.");

    public static readonly Error NotAnImage =
        new("Import.NotAnImage", "The retrieved content is not a supported image.");
}
