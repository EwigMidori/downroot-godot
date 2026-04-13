using Downroot.Core.Ids;

namespace Downroot.Core.World;

public interface IWorldGenContext
{
    WorldSpaceKind WorldSpaceKind { get; }
    int WorldSeed { get; }
    ChunkCoord ChunkCoord { get; }
    int Width { get; }
    int Height { get; }
    WorldTileCoord GetWorldTileCoord(LocalTileCoord coord);
    int GetStableHash(WorldTileCoord coord, int salt);
    float GetStableUnitValue(WorldTileCoord coord, int salt);
    bool HasTerrain(ContentId contentId);
    ContentId? GetBaseTerrain(LocalTileCoord coord);
    ContentId? GetTerrain(LocalTileCoord coord);
    void SetBaseTerrain(LocalTileCoord coord, ContentId terrainId);
    void SetCoverTerrain(LocalTileCoord coord, ContentId? terrainId);
    string GetSurfaceRegion(LocalTileCoord coord);
    bool HasSurfaceRegion(LocalTileCoord coord, string regionKey);
    void SetSurfaceRegion(LocalTileCoord coord, string regionKey);
    bool IsSpawnOccupied(LocalTileCoord coord);
    void AddSpawn(LocalTileCoord coord, ContentId contentId);
}
