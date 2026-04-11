using Downroot.Core.Ids;

namespace Downroot.UI.Presentation;

public sealed record HotbarSlotViewData(
    ContentId? ItemId,
    int Quantity,
    bool IsSelected);
