using FluentAssertions;
using Pinkterest.Application.Photos.Validation;
using Xunit;

namespace Pinkterest.UnitTests.Photos;

public class UploadValidationChainTests
{
    private const long TwoMegabytes = 2L * 1024 * 1024;

    private static UploadValidationContext Context(
        string fileName = "holiday.jpg",
        string contentType = "image/jpeg",
        long sizeBytes = 500_000,
        long maxUploadSizeBytes = TwoMegabytes,
        int dailyUploadLimit = 5,
        int uploadsToday = 0,
        long maxTotalStorageBytes = 50L * 1024 * 1024,
        long totalBytesStored = 0) =>
        new(fileName, contentType, sizeBytes, maxUploadSizeBytes,
            dailyUploadLimit, uploadsToday, maxTotalStorageBytes, totalBytesStored);

    [Fact]
    public void A_valid_upload_passes_every_handler()
    {
        UploadValidationChain.Build().Handle(Context()).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void An_unsupported_content_type_is_rejected_first()
    {
        var result = UploadValidationChain.Build()
            .Handle(Context(contentType: "application/pdf", fileName: "payload.pdf"));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Upload.UnsupportedContentType");
    }

    [Fact]
    public void An_unsupported_extension_is_rejected_even_with_a_valid_content_type()
    {
        var result = UploadValidationChain.Build().Handle(Context(fileName: "payload.exe"));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Upload.UnsupportedExtension");
    }

    [Fact]
    public void An_empty_file_is_rejected()
    {
        var result = UploadValidationChain.Build().Handle(Context(sizeBytes: 0));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Upload.EmptyFile");
    }

    [Fact]
    public void A_file_larger_than_the_package_allows_is_rejected()
    {
        var result = UploadValidationChain.Build().Handle(Context(sizeBytes: TwoMegabytes + 1));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Upload.TooLarge");
    }

    [Fact]
    public void Reaching_the_daily_limit_blocks_further_uploads()
    {
        var result = UploadValidationChain.Build()
            .Handle(Context(dailyUploadLimit: 5, uploadsToday: 5));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Upload.DailyLimitReached");
    }

    [Fact]
    public void Exceeding_total_storage_is_rejected()
    {
        var result = UploadValidationChain.Build().Handle(Context(
            sizeBytes: 2_000_000,
            maxTotalStorageBytes: 3_000_000,
            totalBytesStored: 2_000_000));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Upload.StorageQuotaExceeded");
    }

    [Fact]
    public void The_cheapest_check_runs_before_the_quota_checks()
    {
        var result = UploadValidationChain.Build().Handle(Context(
            contentType: "application/pdf",
            uploadsToday: 99,
            totalBytesStored: long.MaxValue / 2));

        result.Error.Code.Should().Be(
            "Upload.UnsupportedContentType",
            "content type is checked before anything that would need a database round trip");
    }
}
