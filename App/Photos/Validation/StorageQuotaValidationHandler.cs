using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Photos.Validation;

public sealed class StorageQuotaValidationHandler : UploadValidationHandler
{
    protected override Result Validate(UploadValidationContext context) =>
        context.TotalBytesStored + context.SizeBytes <= context.MaxTotalStorageBytes
            ? Result.Success()
            : Result.Failure(PhotoErrors.StorageQuotaExceeded(context.RemainingStorageBytes));
}
