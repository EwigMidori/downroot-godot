using Downroot.Core.Ids;

namespace Downroot.Core.World;

public interface IWorldGenContext
{
    int Width { get; }
    int Height { get; }
    ContentId GetTerrainId(string contentId);
    void SetTerrain(TileCoord coord, ContentId terrainId);
}
