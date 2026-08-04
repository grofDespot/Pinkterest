using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pinkterest.Application.Common.Interfaces;
using Pinkterest.Domain.Entities;

namespace Pinkterest.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IUnitOfWork
{
    public DbSet<Package> Packages => Set<Package>();

    public DbSet<Photo> Photos => Set<Photo>();

    public DbSet<Hashtag> Hashtags => Set<Hashtag>();

    public DbSet<PhotoHashtag> PhotoHashtags => Set<PhotoHashtag>();

    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();

    public DbSet<DailyUsage> DailyUsages => Set<DailyUsage>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<FilterPreset> FilterPresets => Set<FilterPreset>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
