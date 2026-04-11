namespace Downroot.Core.Definitions;

public sealed record CreatureDef(
    Downroot.Core.Ids.ContentId Id,
    string DisplayName,
    string SourcePackId,
    string IdleSpriteSheetPath,
    string RunSpriteSheetPath) : ContentDef(Id, DisplayName, SourcePackId);
