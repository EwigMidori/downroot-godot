using Downroot.Core.Ids;

namespace Downroot.Core.Definitions;

public sealed record RecipeDef(
    ContentId Id,
    string DisplayName,
    string SourcePackId,
    IReadOnlyList<ContentId> IngredientIds,
    ContentId ResultItemId) : ContentDef(Id, DisplayName, SourcePackId);
