using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Photos.Validation;

public sealed class UploadSizeValidationHandler : UploadValidationHandler
{
    protected override Result Validate(UploadValidationContext context)
    {
        if (context.SizeBytes <= 0)
        {
            return Result.Failure(PhotoErrors.EmptyFile);
        }

        return context.SizeBytes <= context.MaxUploadSizeBytes
            ? Result.Success()
            : Result.Failure(PhotoErrors.TooLarge(context.SizeBytes, context.MaxUploadSizeBytes));
    }
}
