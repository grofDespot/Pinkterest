using Pinkterest.Application.Common.Results;

namespace Pinkterest.Application.Photos.Validation;

public abstract class UploadValidationHandler : IUploadValidationHandler
{
    private IUploadValidationHandler? _next;

    public IUploadValidationHandler SetNext(IUploadValidationHandler next)
    {
        _next = next;
        return next;
    }

    public Result Handle(UploadValidationContext context)
    {
        var result = Validate(context);

        return result.IsFailure
            ? result
            : _next?.Handle(context) ?? Result.Success();
    }

    protected abstract Result Validate(UploadValidationContext context);
}
