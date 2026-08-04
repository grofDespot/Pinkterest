using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Photos.Validation;

public sealed class DailyUploadLimitValidationHandler : UploadValidationHandler
{
    protected override Result Validate(UploadValidationContext context) =>
        context.UploadsToday < context.DailyUploadLimit
            ? Result.Success()
            : Result.Failure(PhotoErrors.DailyLimitReached(context.DailyUploadLimit));
}
