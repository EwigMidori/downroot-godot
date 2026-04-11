namespace Downroot.Core.Definitions;

public sealed record RecipeDef(
    Downroot.Core.Ids.ContentId Id,
    string DisplayName,
    string SourcePackId,
    IReadOnlyList<string> IngredientIds,
    string ResultItemId) : ContentDef(Id, DisplayName, SourcePackId);
