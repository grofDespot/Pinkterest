namespace Pinkterest.Domain.Entities;

public class DailyUsage
{
    public Guid UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public DateOnly Date { get; set; }

    public int UploadCount { get; set; }

    public long BytesUploaded { get; set; }
}
