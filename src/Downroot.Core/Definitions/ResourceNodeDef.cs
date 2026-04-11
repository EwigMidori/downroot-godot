using Downroot.Core.Ids;

namespace Downroot.Core.Definitions;

public sealed record ResourceNodeDef(
    ContentId Id,
    string DisplayName,
    string SourcePackId,
    string SpritePath,
    int SpriteWidth,
    int SpriteHeight,
    int AtlasColumn,
    int AtlasRow,
    int MaxDurability,
    IReadOnlyList<ItemAmount> Drops,
    bool InstantPickup = false,
    bool DirectConsume = false,
    int HungerRestore = 0) : ContentDef(Id, DisplayName, SourcePackId);
