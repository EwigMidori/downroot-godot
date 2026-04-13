using Downroot.Core.World;

namespace Downroot.Core.Gameplay;

public sealed record TileSpawn(WorldTileCoord Tile, int PixelOffsetX = 0, int PixelOffsetY = 0);
