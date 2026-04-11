using Downroot.Core.Ids;

namespace Downroot.Core.Definitions;

public sealed record PlaceableDef(
    ContentId Id,
    string DisplayName,
    string SourcePackId,
    string SpritePath,
    int SpriteWidth,
    int SpriteHeight,
    int AtlasColumn = 0,
    int AtlasRow = 0,
    int MaxDurability = 3,
    bool IsCraftingStation = false,
    string? CraftingStationKey = null,
    bool BlocksMovement = false) : ContentDef(Id, DisplayName, SourcePackId);
