using Downroot.Core.Ids;
using Downroot.Core.Save;
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

    public SavedChunkRuntimeData ToSavedData()
    {
        return new SavedChunkRuntimeData
        {
            ChunkX = GeneratedChunk.Coord.X,
            ChunkY = GeneratedChunk.Coord.Y,
            DestroyedNaturalEntityIds = DestroyedNaturalEntityIds.OrderBy(id => id).ToArray(),
            CollectedNaturalDropIds = CollectedNaturalDropIds.OrderBy(id => id).ToArray(),
            RemovedRaisedFeatureTiles = RemovedRaisedFeatureTiles.Select(tile => $"{tile.X},{tile.Y}").OrderBy(value => value).ToArray(),
            RuntimeEntities = _runtimeEntities.Values
                .Where(entity => !entity.Removed)
                .Select(ToSavedRuntimeEntity)
                .ToArray()
        };
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

    public static ChunkRuntimeArchive CreateArchive(SavedChunkRuntimeData savedChunk)
    {
        return new ChunkRuntimeArchive(
            savedChunk.DestroyedNaturalEntityIds.ToArray(),
            savedChunk.CollectedNaturalDropIds.ToArray(),
            savedChunk.RemovedRaisedFeatureTiles.Select(ParseTile).ToArray(),
            savedChunk.RuntimeEntities.Select(FromSavedRuntimeEntity).ToArray());
    }

    private static SavedRuntimeEntityData ToSavedRuntimeEntity(WorldEntityState entity)
    {
        return new SavedRuntimeEntityData
        {
            EntityId = entity.Id.Value.ToByteArray().AsSpan(0, 8).ToArray().Aggregate(0L, (current, next) => (current << 8) | next),
            EntityKind = entity.Kind.ToString(),
            DefinitionId = entity.DefinitionId.Value,
            PositionX = entity.Position.X,
            PositionY = entity.Position.Y,
            Durability = entity.Durability,
            WorldSpaceKind = entity.WorldSpaceKind.ToString(),
            ChunkX = entity.ChunkCoord.X,
            ChunkY = entity.ChunkCoord.Y,
            IsNatural = entity.IsNatural,
            StableNaturalEntityId = entity.StableNaturalEntityId,
            StackCount = entity.StackCount,
            Removed = entity.Removed,
            OpenState = entity.OpenState,
            StorageInventorySlots = entity.StorageInventory?.Slots
                .Select((slot, index) => new SavedInventorySlotData
                {
                    SlotIndex = index,
                    ItemId = slot.ItemId?.Value,
                    Quantity = slot.Quantity
                })
                .ToArray()
        };
    }

    private static WorldEntityState FromSavedRuntimeEntity(SavedRuntimeEntityData savedEntity)
    {
        var worldSpaceKind = Enum.Parse<WorldSpaceKind>(savedEntity.WorldSpaceKind, ignoreCase: true);
        var entityKind = Enum.Parse<WorldEntityKind>(savedEntity.EntityKind, ignoreCase: true);
        var entity = new WorldEntityState(
            entityKind,
            new ContentId(savedEntity.DefinitionId),
            new System.Numerics.Vector2(savedEntity.PositionX, savedEntity.PositionY),
            savedEntity.Durability,
            worldSpaceKind,
            new ChunkCoord(savedEntity.ChunkX, savedEntity.ChunkY),
            savedEntity.IsNatural,
            savedEntity.StableNaturalEntityId,
            savedEntity.StackCount)
        {
            Removed = savedEntity.Removed,
            OpenState = savedEntity.OpenState
        };

        if (savedEntity.StorageInventorySlots is { Count: > 0 } storageSlots)
        {
            var inventory = new InventoryState(storageSlots.Max(slot => slot.SlotIndex) + 1);
            foreach (var slot in storageSlots)
            {
                inventory.SetSlot(slot.SlotIndex, slot.ItemId is null ? null : new ContentId(slot.ItemId), slot.Quantity);
            }

            entity.StorageInventory = inventory;
        }

        return entity;
    }

    private static WorldTileCoord ParseTile(string value)
    {
        var parts = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return new WorldTileCoord(int.Parse(parts[0]), int.Parse(parts[1]));
    }
}

public sealed record ChunkRuntimeArchive(
    IReadOnlyCollection<string> DestroyedNaturalEntityIds,
    IReadOnlyCollection<string> CollectedNaturalDropIds,
    IReadOnlyCollection<WorldTileCoord> RemovedRaisedFeatureTiles,
    IReadOnlyCollection<WorldEntityState> RuntimeEntities);
