using Downroot.Core.Ids;

namespace Downroot.Core.Definitions;

public sealed record RaisedFeatureDef(
    ContentId Id,
    string DisplayName,
    string SourcePackId,
    string TexturePath,
    int TileWidth,
    int TileHeight,
    int AutoTileColumnCount,
    int MaxDurability,
    IReadOnlyList<ItemAmount> Drops,
    bool BlocksMovement = true) : ContentDef(Id, DisplayName, SourcePackId);
