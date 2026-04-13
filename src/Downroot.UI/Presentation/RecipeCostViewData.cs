using Downroot.Core.Ids;

namespace Downroot.UI.Presentation;

public sealed record RecipeCostViewData(
    ContentId ItemId,
    string ItemName,
    int Amount,
    bool IsSatisfied,
    int MissingAmount);
