using Downroot.Core.Definitions;

namespace Downroot.Core.World;

public interface IWorldGenContext
{
    int Width { get; }
    int Height { get; }
    TerrainDef GetTerrain(string contentId);
    void SetTerrain(TileCoord coord, TerrainDef terrainDef);
}
