using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pinkterest.Domain.Entities;

namespace Pinkterest.Infrastructure.Persistence.Configurations;

public sealed class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.Property(a => a.UserName).HasMaxLength(256).IsRequired();
        builder.Property(a => a.Action).HasMaxLength(128).IsRequired();
        builder.Property(a => a.EntityType).HasMaxLength(128);
        builder.Property(a => a.EntityId).HasMaxLength(64);
        builder.Property(a => a.IpAddress).HasMaxLength(64);
        builder.Property(a => a.DetailsJson).HasColumnType("jsonb");

        builder.HasIndex(a => a.OccurredUtc);
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.Action);
    }
}
