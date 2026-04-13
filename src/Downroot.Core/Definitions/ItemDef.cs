using Downroot.Core.Ids;

namespace Downroot.Core.Definitions;

public sealed record ItemDef(
    ContentId Id,
    string DisplayName,
    string SourcePackId,
    string IconPath,
    int IconWidth,
    int IconHeight,
    int MaxStack,
    ContentId? PlaceableId = null,
    int HungerRestore = 0,
    int HealthRestore = 0,
    float TreeBreakSpeedMultiplier = 1f,
    int MeleeDamage = 0) : ContentDef(Id, DisplayName, SourcePackId);
