using Downroot.Core.Ids;

namespace Downroot.Core.Definitions;

public sealed record TerrainDef(
    ContentId Id,
    string DisplayName,
    string SourcePackId,
    string TexturePath,
    int TileWidth,
    int TileHeight,
    int AtlasColumn,
    int AtlasRow) : ContentDef(Id, DisplayName, SourcePackId);
