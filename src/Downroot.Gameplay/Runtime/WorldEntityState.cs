using System.Numerics;
using Downroot.Core.Ids;
using Downroot.Core.World;

namespace Downroot.Gameplay.Runtime;

public sealed class WorldEntityState
{
    public WorldEntityState(
        WorldEntityKind kind,
        ContentId definitionId,
        Vector2 position,
        int durability,
        WorldSpaceKind worldSpaceKind,
        ChunkCoord chunkCoord,
        bool isNatural = false,
        string? stableNaturalEntityId = null,
        int stackCount = 1,
        EntityId? entityId = null)
    {
        Id = entityId ?? EntityId.New();
        Kind = kind;
        DefinitionId = definitionId;
        Position = position;
        Durability = durability;
        WorldSpaceKind = worldSpaceKind;
        ChunkCoord = chunkCoord;
        IsNatural = isNatural;
        StableNaturalEntityId = stableNaturalEntityId;
        StackCount = stackCount;
    }

    public EntityId Id { get; }
    public WorldEntityKind Kind { get; }
    public ContentId DefinitionId { get; }
    public Vector2 Position { get; set; }
    public int Durability { get; set; }
    public WorldSpaceKind WorldSpaceKind { get; set; }
    public ChunkCoord ChunkCoord { get; set; }
    public bool IsNatural { get; }
    public string? StableNaturalEntityId { get; }
    public int StackCount { get; set; }
    public bool Removed { get; set; }
    public bool OpenState { get; set; }
    public float DamageAccumulator { get; set; }
    public float AiAccumulator { get; set; }
    public float HitFlashSeconds { get; set; }

    public WorldEntityState Clone()
    {
        return new WorldEntityState(
            Kind,
            DefinitionId,
            Position,
            Durability,
            WorldSpaceKind,
            ChunkCoord,
            IsNatural,
            StableNaturalEntityId,
            StackCount,
            Id)
        {
            Removed = Removed,
            OpenState = OpenState,
            DamageAccumulator = DamageAccumulator,
            AiAccumulator = AiAccumulator,
            HitFlashSeconds = HitFlashSeconds
        };
    }
}
