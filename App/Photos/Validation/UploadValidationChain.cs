namespace Pinkterest.Application.Photos.Validation;

public static class UploadValidationChain
{
    public static IUploadValidationHandler Build()
    {
        var head = new ContentTypeValidationHandler();

        head.SetNext(new FileExtensionValidationHandler())
            .SetNext(new UploadSizeValidationHandler())
            .SetNext(new DailyUploadLimitValidationHandler())
            .SetNext(new StorageQuotaValidationHandler());

        return head;
    }
}
