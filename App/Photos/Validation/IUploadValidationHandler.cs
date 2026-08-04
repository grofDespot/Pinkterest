using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Photos.Validation;

public interface IUploadValidationHandler
{
    IUploadValidationHandler SetNext(IUploadValidationHandler next);

    Result Handle(UploadValidationContext context);
}
