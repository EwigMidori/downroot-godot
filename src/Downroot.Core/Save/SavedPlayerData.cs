namespace Downroot.Core.Save;

public sealed class SavedPlayerData
{
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float FacingX { get; set; }
    public float FacingY { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Hunger { get; set; }
    public int MaxHunger { get; set; }
    public int SelectedHotbarIndex { get; set; }
    public IReadOnlyList<SavedInventorySlotData> InventorySlots { get; set; } = [];
}
