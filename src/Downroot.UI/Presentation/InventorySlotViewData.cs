using Downroot.Core.Ids;

namespace Downroot.UI.Presentation;

public sealed record InventorySlotViewData(
    ContentId? ItemId,
    int Quantity);
