namespace Downroot.UI.Presentation;

public enum CraftModeIconKind
{
    Handcraft,
    Workbench,
    Furnace
}

public sealed record CraftingPanelViewData(
    bool IsVisible,
    string CraftModeLabel,
    CraftModeIconKind CraftModeIcon,
    IReadOnlyList<CraftRecipeViewData> Recipes,
    IReadOnlyList<InventorySlotViewData> InventorySlots,
    string StorageTitle,
    IReadOnlyList<InventorySlotViewData> StorageSlots);
