using Downroot.Core.Ids;
using Downroot.Core.World;
using Downroot.World.Models;

namespace Downroot.Gameplay.Runtime;

public sealed class ChunkRuntimeState
{
    private readonly Dictionary<string, WorldEntityState> _naturalEntities;
    private readonly Dictionary<EntityId, WorldEntityState> _runtimeEntities;

    public ChunkRuntimeState(GeneratedChunk generatedChunk, IReadOnlyDictionary<string, WorldEntityState>? naturalEntities = null, IReadOnlyDictionary<EntityId, WorldEntityState>? runtimeEntities = null)
    {
        GeneratedChunk = generatedChunk;
        _naturalEntities = naturalEntities is null
            ? new Dictionary<string, WorldEntityState>(StringComparer.Ordinal)
            : new Dictionary<string, WorldEntityState>(naturalEntities, StringComparer.Ordinal);
        _runtimeEntities = runtimeEntities is null
            ? new Dictionary<EntityId, WorldEntityState>()
            : new Dictionary<EntityId, WorldEntityState>(runtimeEntities);
    }

    public GeneratedChunk GeneratedChunk { get; }
    public IReadOnlyDictionary<string, WorldEntityState> NaturalEntities => _naturalEntities;
    public IReadOnlyDictionary<EntityId, WorldEntityState> RuntimeEntities => _runtimeEntities;
    public HashSet<string> DestroyedNaturalEntityIds { get; } = new(StringComparer.Ordinal);
    public HashSet<string> CollectedNaturalDropIds { get; } = new(StringComparer.Ordinal);
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

    public ChunkRuntimeState CreateArchiveCopy()
    {
        var copy = new ChunkRuntimeState(GeneratedChunk, _naturalEntities, _runtimeEntities);
        copy.DestroyedNaturalEntityIds.UnionWith(DestroyedNaturalEntityIds);
        copy.CollectedNaturalDropIds.UnionWith(CollectedNaturalDropIds);
        return copy;
    }
}
