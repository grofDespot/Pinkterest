using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Photos.Validation;

public sealed class ContentTypeValidationHandler : UploadValidationHandler
{
    public static readonly IReadOnlySet<string> AllowedContentTypes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/bmp"
        };

    protected override Result Validate(UploadValidationContext context) =>
        AllowedContentTypes.Contains(context.ContentType)
            ? Result.Success()
            : Result.Failure(PhotoErrors.UnsupportedContentType);
}
