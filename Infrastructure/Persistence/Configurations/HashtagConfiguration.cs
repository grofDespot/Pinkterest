using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pinkterest.Domain.Entities;

namespace Pinkterest.Infrastructure.Persistence.Configurations;

public sealed class HashtagConfiguration : IEntityTypeConfiguration<Hashtag>
{
    public void Configure(EntityTypeBuilder<Hashtag> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Ignore(h => h.DomainEvents);

        builder.Property(h => h.Name).HasMaxLength(64).IsRequired();
        builder.HasIndex(h => h.Name).IsUnique();
    }
}
