using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Photos;

public static class PhotoErrors
{
    public static readonly Error UnsupportedContentType =
        new("Upload.UnsupportedContentType", "Only JPEG, PNG and BMP images can be uploaded.");

    public static readonly Error UnsupportedExtension =
        new("Upload.UnsupportedExtension", "The file extension is not one of .jpg, .jpeg, .png or .bmp.");

    public static readonly Error EmptyFile =
        new("Upload.EmptyFile", "The selected file is empty.");

    public static readonly Error NotAnImage =
        new("Upload.NotAnImage", "The file could not be read as an image.");

    public static Error TooLarge(long sizeBytes, long limitBytes) =>
        new("Upload.TooLarge", $"The file is {sizeBytes / 1024} KB but your package allows {limitBytes / 1024} KB per photo.");

    public static Error DailyLimitReached(int limit) =>
        new("Upload.DailyLimitReached", $"You have reached your daily upload limit of {limit} photos.");

    public static Error StorageQuotaExceeded(long remainingBytes) =>
        new("Upload.StorageQuotaExceeded", $"This upload exceeds your storage quota. You have {remainingBytes / 1024} KB left.");
}
