using Downroot.Core.Ids;

namespace Downroot.Core.Definitions;

public sealed record RecipeDef(
    ContentId Id,
    string DisplayName,
    string SourcePackId,
    IReadOnlyList<ItemAmount> Ingredients,
    ItemAmount Result,
    string? RequiredStationKey = null,
    float CraftDurationSeconds = 0f,
    IReadOnlyList<ItemAmount>? ExtraResults = null) : ContentDef(Id, DisplayName, SourcePackId);
