using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pinkterest.Domain.Entities;

namespace Pinkterest.Infrastructure.Persistence.Configurations;

public sealed class PhotoConfiguration : IEntityTypeConfiguration<Photo>
{
    public void Configure(EntityTypeBuilder<Photo> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Ignore(p => p.DomainEvents);

        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.OriginalFileName).HasMaxLength(260).IsRequired();
        builder.Property(p => p.StorageKey).HasMaxLength(512).IsRequired();
        builder.Property(p => p.ThumbnailKey).HasMaxLength(512);
        builder.Property(p => p.ContentType).HasMaxLength(128).IsRequired();

        builder.HasOne(p => p.Owner)
            .WithMany(u => u.Photos)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.UploadedUtc);
        builder.HasIndex(p => p.OwnerId);
        builder.HasIndex(p => p.SizeBytes);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
