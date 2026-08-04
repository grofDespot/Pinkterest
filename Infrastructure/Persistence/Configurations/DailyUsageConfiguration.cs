using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pinkterest.Domain.Entities;

namespace Pinkterest.Infrastructure.Persistence.Configurations;

public sealed class DailyUsageConfiguration : IEntityTypeConfiguration<DailyUsage>
{
    public void Configure(EntityTypeBuilder<DailyUsage> builder)
    {
        builder.HasKey(d => new { d.UserId, d.Date });

        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.Date);
    }
}
