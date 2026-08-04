using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pinkterest.Domain.Entities;

namespace Pinkterest.Infrastructure.Persistence.Configurations;

public sealed class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Ignore(p => p.DomainEvents);

        builder.Property(p => p.Name).HasMaxLength(32).IsRequired();
        builder.Property(p => p.Tier).HasConversion<int>();
        builder.Property(p => p.MonthlyPrice).HasPrecision(8, 2);

        builder.HasIndex(p => p.Tier).IsUnique();
    }
}
