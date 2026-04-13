using Downroot.Core.Ids;

namespace Downroot.Core.World;

public interface IWorldGenContext
{
    int Width { get; }
    int Height { get; }
    bool HasTerrain(ContentId contentId);
    ContentId? GetBaseTerrain(TileCoord coord);
    ContentId? GetTerrain(TileCoord coord);
    void SetBaseTerrain(TileCoord coord, ContentId terrainId);
    void SetCoverTerrain(TileCoord coord, ContentId? terrainId);
    string GetSurfaceRegion(TileCoord coord);
    bool HasSurfaceRegion(TileCoord coord, string regionKey);
    void SetSurfaceRegion(TileCoord coord, string regionKey);
    bool IsSpawnOccupied(TileCoord coord);
    void AddSpawn(TileCoord coord, ContentId contentId);
}
