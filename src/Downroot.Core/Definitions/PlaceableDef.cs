namespace Downroot.Core.Definitions;

public sealed record PlaceableDef(
    Downroot.Core.Ids.ContentId Id,
    string DisplayName,
    string SourcePackId,
    string SpritePath) : ContentDef(Id, DisplayName, SourcePackId);
