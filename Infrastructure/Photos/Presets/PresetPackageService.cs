using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Pinkterest.Application.Common.Auditing;
using Pinkterest.Application.Common.Results;
using Pinkterest.Application.Photos.Presets;
using Pinkterest.CrossCutting.Auditing;
using Pinkterest.Domain.Entities;
using Pinkterest.Infrastructure.Persistence;

namespace Pinkterest.Infrastructure.Photos.Presets;

public sealed class PresetPackageService(
    ApplicationDbContext context,
    IFilterPresetService presetService,
    PresetPackageFormat format,
    TimeProvider timeProvider) : IPresetPackageService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    [Audited(AuditActions.PresetSaved, EntityType = nameof(FilterPreset))]
    public async Task<Result<byte[]>> ExportAsync(
        Guid presetId,
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        var preset = await context.FilterPresets
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == presetId && p.OwnerId == ownerId, cancellationToken);

        if (preset is null)
        {
            return Result.Failure<byte[]>(FilterPresetErrors.NotFound);
        }

        var definition = JsonSerializer.Deserialize<FilterPresetDefinition>(preset.DefinitionJson, SerializerOptions)
            ?? new FilterPresetDefinition(default, null, null, []);

        return Result.Success(format.Write(definition));
    }

    [Audited(AuditActions.PresetSaved, EntityType = nameof(FilterPreset))]
    public async Task<Result<Guid>> ImportAsync(
        Guid ownerId,
        byte[] package,
        CancellationToken cancellationToken = default)
    {
        var read = format.Read(package);

        if (read.IsFailure)
        {
            return Result.Failure<Guid>(read.Error);
        }

        var safe = read.Value.WithKnownFiltersOnly();

        return await presetService.SaveAsync(
            ownerId,
            $"Imported {timeProvider.GetUtcNow():yyyy-MM-dd HH:mm}",
            safe.ToOptions(),
            cancellationToken);
    }
}
