using Amazon.S3;
using Castle.DynamicProxy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pinkterest.Application.Accounts;
using Pinkterest.Application.Accounts.External;
using Pinkterest.Application.Admin.Models;
using Pinkterest.Application.Admin.Requests;
using Pinkterest.Application.Common.Auditing;
using Pinkterest.Application.Common.Events;
using Pinkterest.Application.Common.Interfaces;
using Pinkterest.Application.Common.Mediation;
using Pinkterest.Application.Common.Results;
using Pinkterest.Application.Packages;
using Pinkterest.Application.Photos;
using Pinkterest.Application.Photos.Download;
using Pinkterest.Application.Photos.Presets;
using Pinkterest.Application.Photos.Processing;
using Pinkterest.Application.Photos.Search;
using Pinkterest.Application.Photos.Storage;
using Pinkterest.Application.Usage;
using Pinkterest.Domain.Entities;
using Pinkterest.Domain.Events;
using Pinkterest.Infrastructure.Admin;
using Pinkterest.Infrastructure.Auditing;
using Pinkterest.Infrastructure.Events;
using Pinkterest.Infrastructure.Identity;
using Pinkterest.Infrastructure.Interception;
using Pinkterest.Infrastructure.Mediation;
using Pinkterest.Infrastructure.Observability;
using Pinkterest.Infrastructure.Packages;
using Pinkterest.Infrastructure.Persistence;
using Pinkterest.Infrastructure.Persistence.Seeding;
using Pinkterest.Infrastructure.Photos;
using Pinkterest.Infrastructure.Storage;
using Pinkterest.Infrastructure.Usage;

namespace Pinkterest.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddSingleton(TimeProvider.System);
        services.AddMemoryCache(options => options.SizeLimit = 64L * 1024 * 1024);

        services.AddOptions<SeedOptions>().Bind(configuration.GetSection(SeedOptions.SectionName));
        services.AddOptions<StorageOptions>().Bind(configuration.GetSection(StorageOptions.SectionName));
        services.AddOptions<S3StorageOptions>().Bind(configuration.GetSection(S3StorageOptions.SectionName));
        services.AddScoped<DatabaseSeeder>();

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedAccount = false;
            options.Password.RequiredLength = 12;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddExternalAuthentication(configuration);

        services.AddScoped<IAsyncInterceptor, AuditInterceptor>();
        services.AddScoped<IAsyncInterceptor, MetricsInterceptor>();

        services.AddInterceptedScoped<IAccountService, AccountService>();
        services.AddInterceptedScoped<IExternalAuthenticationService, ExternalAuthenticationService>();
        services.AddScoped<IPackageCatalog, PackageCatalog>();
        services.AddScoped<IUsageQuery, UsageQuery>();

        services.AddScoped<LocalFileSystemPhotoStorage>();
        services.AddScoped<S3PhotoStorage>();
        services.AddSingleton<IAmazonS3>(sp =>
        {
            var s3 = sp.GetRequiredService<IOptions<S3StorageOptions>>().Value;

            var config = new AmazonS3Config
            {
                ForcePathStyle = s3.ForcePathStyle,
                AuthenticationRegion = s3.Region
            };

            if (!string.IsNullOrWhiteSpace(s3.ServiceUrl))
            {
                config.ServiceURL = s3.ServiceUrl;
            }

            return new AmazonS3Client(s3.AccessKey, s3.SecretKey, config);
        });
        services.AddScoped<IPhotoStorageFactory, PhotoStorageFactory>();
        services.AddScoped<IPhotoStorage>(sp => new CachingPhotoStorageProxy(
            sp.GetRequiredService<IPhotoStorageFactory>().Create(),
            sp.GetRequiredService<IMemoryCache>()));
        services.AddSingleton<IImageProcessor, ImageProcessor>();
        services.AddInterceptedScoped<IPhotoUploadService, PhotoUploadService>();
        services.AddScoped<IPhotoRepository, PhotoRepository>();
        services.AddInterceptedScoped<IPhotoEditService, PhotoEditService>();
        services.AddInterceptedScoped<IPhotoSearchService, PhotoSearchService>();
        services.AddInterceptedScoped<IPhotoDownloadService, PhotoDownloadService>();
        services.AddInterceptedScoped<IFilterPresetService, FilterPresetService>();

        services.AddScoped<IAuditLog, AuditLog>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IDomainEventHandler<PhotoUploadedEvent>, LogPhotoUploadedHandler>();
        services.AddScoped<IDomainEventHandler<PhotoUploadedEvent>, PhotoUploadMetricsHandler>();

        services.AddScoped<ISender, Sender>();
        services.AddScoped<IRequestHandler<GetAdminStatisticsQuery, AdminStatistics>, GetAdminStatisticsHandler>();
        services.AddScoped<IRequestHandler<GetUsersQuery, IReadOnlyList<AdminUserSummary>>, GetUsersHandler>();
        services.AddScoped<IRequestHandler<GetUserDetailQuery, AdminUserDetail?>, GetUserDetailHandler>();
        services.AddScoped<IRequestHandler<UpdateUserCommand, Result>, UpdateUserHandler>();
        services.AddScoped<IRequestHandler<GetAuditLogQuery, AuditLogPage>, GetAuditLogHandler>();
        services.AddScoped<IRequestHandler<GetManagedPhotosQuery, PhotoSearchResult>, GetManagedPhotosHandler>();
        services.AddScoped<IRequestHandler<DeletePhotoCommand, Result>, DeletePhotoHandler>();

        services.AddInterceptedScoped<IPackageChangeService, PackageChangeService>();
        services.AddSingleton<StorageMetrics>();
        services.AddHostedService<StorageMetricsInitializer>();
        services.AddHostedService<PendingPackageChangeWorker>();

        return services;
    }

    private static IServiceCollection AddExternalAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(ExternalAuthenticationOptions.SectionName)
            .Get<ExternalAuthenticationOptions>() ?? new ExternalAuthenticationOptions();

        var builder = services.AddAuthentication();

        if (options.Google.IsConfigured)
        {
            builder.AddGoogle(google =>
            {
                google.ClientId = options.Google.ClientId;
                google.ClientSecret = options.Google.ClientSecret;
                google.CallbackPath = "/signin-google";
                google.SaveTokens = false;
            });
        }

        if (options.GitHub.IsConfigured)
        {
            builder.AddGitHub(github =>
            {
                github.ClientId = options.GitHub.ClientId;
                github.ClientSecret = options.GitHub.ClientSecret;
                github.CallbackPath = "/signin-github";
                github.Scope.Add("user:email");
                github.SaveTokens = false;
            });
        }

        return services;
    }
}
