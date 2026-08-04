namespace Pinkterest.Application.Photos.Validation;

public sealed record UploadValidationContext(
    string FileName,
    string ContentType,
    long SizeBytes,
    long MaxUploadSizeBytes,
    int DailyUploadLimit,
    int UploadsToday,
    long MaxTotalStorageBytes,
    long TotalBytesStored)
{
    public long RemainingStorageBytes => Math.Max(0, MaxTotalStorageBytes - TotalBytesStored);
}
