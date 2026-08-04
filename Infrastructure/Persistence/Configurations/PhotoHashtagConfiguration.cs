using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pinkterest.Domain.Entities;

namespace Pinkterest.Infrastructure.Persistence.Configurations;

public sealed class PhotoHashtagConfiguration : IEntityTypeConfiguration<PhotoHashtag>
{
    public void Configure(EntityTypeBuilder<PhotoHashtag> builder)
    {
        builder.HasKey(ph => new { ph.PhotoId, ph.HashtagId });

        builder.HasOne(ph => ph.Photo)
            .WithMany(p => p.PhotoHashtags)
            .HasForeignKey(ph => ph.PhotoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ph => ph.Hashtag)
            .WithMany(h => h.PhotoHashtags)
            .HasForeignKey(ph => ph.HashtagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ph => ph.HashtagId);
    }
}
