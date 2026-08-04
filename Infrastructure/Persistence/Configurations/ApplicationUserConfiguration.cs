using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pinkterest.Domain.Entities;

namespace Pinkterest.Infrastructure.Persistence.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.DisplayName).HasMaxLength(128).IsRequired();

        builder.HasOne(u => u.Package)
            .WithMany(p => p.Subscribers)
            .HasForeignKey(u => u.PackageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.PendingPackage)
            .WithMany()
            .HasForeignKey(u => u.PendingPackageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
