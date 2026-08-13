using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pinkterest.Application.Common.Auditing;
using Pinkterest.Application.Common.Results;
using Pinkterest.Application.Photos.Presets;
using Pinkterest.Application.Photos.Processing;
using Pinkterest.CrossCutting.Auditing;
using Pinkterest.Domain.Entities;
using Pinkterest.Infrastructure.Persistence;

namespace Pinkterest.Infrastructure.Photos;

public sealed class FilterPresetService(
    ApplicationDbContext context,
    TimeProvider timeProvider,
    ILogger<FilterPresetService> logger) : IFilterPresetService
{
    private const int MaxPresetsPerUser = 20;

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        MaxDepth = 8
    };

    public async Task<IReadOnlyList<FilterPresetDto>> ListAsync(
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        var presets = await context.FilterPresets
            .AsNoTracking()
            .Where(preset => preset.OwnerId == ownerId)
            .OrderBy(preset => preset.Name)
            .ToListAsync(cancellationToken);

        return presets
            .Select(preset => new
            {
                preset.Id,
                preset.Name,
                preset.CreatedUtc,
                Definition = Deserialize(preset.Id, preset.DefinitionJson)
            })
            .Where(row => row.Definition is not null)
            .Select(row => new FilterPresetDto(row.Id, row.Name, row.Definition!.ToOptions(), row.CreatedUtc))
            .ToList();
    }

    public async Task<Result<ImageProcessingOptions>> GetOptionsAsync(
        Guid presetId,
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        var preset = await context.FilterPresets
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == presetId && p.OwnerId == ownerId, cancellationToken);

        if (preset is null)
        {
            return Result.Failure<ImageProcessingOptions>(Error.NotFound("Preset"));
        }

        var definition = Deserialize(preset.Id, preset.DefinitionJson);

        return definition is null
            ? Result.Failure<ImageProcessingOptions>(FilterPresetErrors.UnknownFilter)
            : Result.Success(definition.ToOptions());
    }

    [Audited(AuditActions.PresetSaved, EntityType = nameof(FilterPreset))]
    public async Task<Result<Guid>> SaveAsync(
        Guid ownerId,
        string name,
        ImageProcessingOptions options,
        CancellationToken cancellationToken = default)
    {
        name = name.Trim();

        if (name.Length == 0)
        {
            return Result.Failure<Guid>(FilterPresetErrors.NameRequired);
        }

        var definition = FilterPresetDefinition.From(options);

        if (definition.Filters.Count != definition.WithKnownFiltersOnly().Filters.Count)
        {
            return Result.Failure<Guid>(FilterPresetErrors.UnknownFilter);
        }

        var existing = await context.FilterPresets
            .Where(preset => preset.OwnerId == ownerId)
            .ToListAsync(cancellationToken);

        if (existing.Any(preset => string.Equals(preset.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            return Result.Failure<Guid>(FilterPresetErrors.NameTaken);
        }

        if (existing.Count >= MaxPresetsPerUser)
        {
            return Result.Failure<Guid>(FilterPresetErrors.LimitReached(MaxPresetsPerUser));
        }

        var preset = new FilterPreset
        {
            OwnerId = ownerId,
            Name = name,
            DefinitionJson = JsonSerializer.Serialize(definition, SerializerOptions),
            CreatedUtc = timeProvider.GetUtcNow()
        };

        context.FilterPresets.Add(preset);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(preset.Id);
    }

    [Audited(AuditActions.PresetDeleted, EntityType = nameof(FilterPreset))]
    public async Task<Result> DeleteAsync(
        Guid presetId,
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        var preset = await context.FilterPresets
            .SingleOrDefaultAsync(p => p.Id == presetId && p.OwnerId == ownerId, cancellationToken);

        if (preset is null)
        {
            return Result.Failure(Error.NotFound("Preset"));
        }

        context.FilterPresets.Remove(preset);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private FilterPresetDefinition? Deserialize(Guid presetId, string json)
    {
        try
        {
            var definition = JsonSerializer.Deserialize<FilterPresetDefinition>(json, SerializerOptions);

            if (definition is null)
            {
                return null;
            }

            var sanitised = definition.WithKnownFiltersOnly();

            if (sanitised.Filters.Count != definition.Filters.Count)
            {
                logger.LogWarning(
                    "Preset {PresetId} referenced filters this application does not provide; they were dropped.",
                    presetId);
            }

            return sanitised;
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Preset {PresetId} holds a definition that could not be read.", presetId);
            return null;
        }
    }
}
