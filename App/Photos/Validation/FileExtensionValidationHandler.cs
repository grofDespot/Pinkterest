using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Photos.Validation;

public sealed class FileExtensionValidationHandler : UploadValidationHandler
{
    private static readonly IReadOnlySet<string> AllowedExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".bmp" };

    protected override Result Validate(UploadValidationContext context)
    {
        var extension = Path.GetExtension(context.FileName);

        return AllowedExtensions.Contains(extension)
            ? Result.Success()
            : Result.Failure(PhotoErrors.UnsupportedExtension);
    }
}
