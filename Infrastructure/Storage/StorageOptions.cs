namespace Pinkterest.Infrastructure.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public StorageProvider Provider { get; set; } = StorageProvider.Local;

    public string LocalRootPath { get; set; } = "storage";
}
