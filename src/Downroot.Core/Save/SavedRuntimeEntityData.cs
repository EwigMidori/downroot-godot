namespace Downroot.Core.Save;

public sealed class SavedRuntimeEntityData
{
    public long EntityId { get; set; }
    public string EntityKind { get; set; } = string.Empty;
    public string DefinitionId { get; set; } = string.Empty;
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public int Durability { get; set; }
    public string WorldSpaceKind { get; set; } = string.Empty;
    public int ChunkX { get; set; }
    public int ChunkY { get; set; }
    public bool IsNatural { get; set; }
    public string? StableNaturalEntityId { get; set; }
    public int StackCount { get; set; }
    public bool Removed { get; set; }
    public bool OpenState { get; set; }
    public IReadOnlyList<SavedInventorySlotData>? StorageInventorySlots { get; set; }
}
