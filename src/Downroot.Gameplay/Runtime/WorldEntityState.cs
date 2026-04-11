using System.Numerics;
using Downroot.Core.Ids;

namespace Downroot.Gameplay.Runtime;

public sealed class WorldEntityState
{
    public WorldEntityState(WorldEntityKind kind, ContentId definitionId, Vector2 position, int durability, int stackCount = 1)
    {
        Id = EntityId.New();
        Kind = kind;
        DefinitionId = definitionId;
        Position = position;
        Durability = durability;
        StackCount = stackCount;
    }

    public EntityId Id { get; }
    public WorldEntityKind Kind { get; }
    public ContentId DefinitionId { get; }
    public Vector2 Position { get; set; }
    public int Durability { get; set; }
    public int StackCount { get; set; }
    public bool Removed { get; set; }
    public bool OpenState { get; set; }
    public float DamageAccumulator { get; set; }
    public float AiAccumulator { get; set; }
}
