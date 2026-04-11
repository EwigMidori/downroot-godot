namespace Downroot.Core.Definitions;

public sealed record ItemDef(
    Downroot.Core.Ids.ContentId Id,
    string DisplayName,
    string SourcePackId,
    string IconPath) : ContentDef(Id, DisplayName, SourcePackId);
