using Downroot.Core.Ids;

namespace Downroot.Core.World;

public interface IWorldGenContext
{
    int Width { get; }
    int Height { get; }
    bool HasTerrain(ContentId contentId);
    ContentId? GetTerrain(TileCoord coord);
    void SetTerrain(TileCoord coord, ContentId terrainId);
    void AddSpawn(TileCoord coord, ContentId contentId);
}
