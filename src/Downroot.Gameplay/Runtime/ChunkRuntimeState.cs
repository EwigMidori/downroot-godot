using Downroot.Core.Ids;
using Downroot.Core.World;
using Downroot.World.Models;

namespace Downroot.Gameplay.Runtime;

public sealed class ChunkRuntimeState
{
    private readonly Dictionary<string, WorldEntityState> _naturalEntities;
    private readonly Dictionary<EntityId, WorldEntityState> _runtimeEntities;

    public ChunkRuntimeState(GeneratedChunk generatedChunk)
    {
        GeneratedChunk = generatedChunk;
        _naturalEntities = new Dictionary<string, WorldEntityState>(StringComparer.Ordinal);
        _runtimeEntities = new Dictionary<EntityId, WorldEntityState>();
    }

    public GeneratedChunk GeneratedChunk { get; }
    public IReadOnlyDictionary<string, WorldEntityState> NaturalEntities => _naturalEntities;
    public IReadOnlyDictionary<EntityId, WorldEntityState> RuntimeEntities => _runtimeEntities;
    public HashSet<string> DestroyedNaturalEntityIds { get; } = new(StringComparer.Ordinal);
    public HashSet<string> CollectedNaturalDropIds { get; } = new(StringComparer.Ordinal);
    public HashSet<WorldTileCoord> RemovedRaisedFeatureTiles { get; } = [];
    public IEnumerable<WorldEntityState> Entities => _naturalEntities.Values.Concat(_runtimeEntities.Values);

    public void AddNaturalEntity(WorldEntityState entity)
    {
        if (entity.StableNaturalEntityId is null)
        {
            throw new InvalidOperationException("Natural entities must carry a stable natural entity id.");
        }

        _naturalEntities[entity.StableNaturalEntityId] = entity;
    }

    public void AddRuntimeEntity(WorldEntityState entity) => _runtimeEntities[entity.Id] = entity;

    public bool RemoveEntity(WorldEntityState entity)
    {
        if (entity.IsNatural && entity.StableNaturalEntityId is not null)
        {
            DestroyedNaturalEntityIds.Add(entity.StableNaturalEntityId);
            return _naturalEntities.Remove(entity.StableNaturalEntityId);
        }

        return _runtimeEntities.Remove(entity.Id);
    }

    public bool TakeRuntimeEntity(EntityId entityId, out WorldEntityState? entity) => _runtimeEntities.Remove(entityId, out entity);

    public bool HasPersistentState()
    {
        return DestroyedNaturalEntityIds.Count > 0
            || CollectedNaturalDropIds.Count > 0
            || RemovedRaisedFeatureTiles.Count > 0
            || _runtimeEntities.Values.Any(entity => !entity.Removed);
    }

    public ChunkRuntimeArchive CreateArchive()
    {
        return new ChunkRuntimeArchive(
            DestroyedNaturalEntityIds.ToArray(),
            CollectedNaturalDropIds.ToArray(),
            RemovedRaisedFeatureTiles.ToArray(),
            _runtimeEntities.Values
                .Where(entity => !entity.Removed)
                .Select(entity => entity.Clone())
                .ToArray());
    }

    public void ApplyArchive(ChunkRuntimeArchive archive)
    {
        DestroyedNaturalEntityIds.UnionWith(archive.DestroyedNaturalEntityIds);
        CollectedNaturalDropIds.UnionWith(archive.CollectedNaturalDropIds);
        RemovedRaisedFeatureTiles.UnionWith(archive.RemovedRaisedFeatureTiles);

        foreach (var destroyedNaturalEntityId in archive.DestroyedNaturalEntityIds)
        {
            _naturalEntities.Remove(destroyedNaturalEntityId);
        }

        foreach (var runtimeEntity in archive.RuntimeEntities.Where(entity => !entity.Removed))
        {
            _runtimeEntities[runtimeEntity.Id] = runtimeEntity.Clone();
        }
    }
}

public sealed record ChunkRuntimeArchive(
    IReadOnlyCollection<string> DestroyedNaturalEntityIds,
    IReadOnlyCollection<string> CollectedNaturalDropIds,
    IReadOnlyCollection<WorldTileCoord> RemovedRaisedFeatureTiles,
    IReadOnlyCollection<WorldEntityState> RuntimeEntities);
