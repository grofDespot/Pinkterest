using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pinkterest.Domain.Entities;

namespace Pinkterest.Infrastructure.Persistence.Configurations;

public sealed class FilterPresetConfiguration : IEntityTypeConfiguration<FilterPreset>
{
    public void Configure(EntityTypeBuilder<FilterPreset> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Ignore(f => f.DomainEvents);

        builder.Property(f => f.Name).HasMaxLength(64).IsRequired();
        builder.Property(f => f.DefinitionJson).HasColumnType("jsonb").IsRequired();

        builder.HasOne(f => f.Owner)
            .WithMany(u => u.FilterPresets)
            .HasForeignKey(f => f.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => new { f.OwnerId, f.Name }).IsUnique();
    }
}
