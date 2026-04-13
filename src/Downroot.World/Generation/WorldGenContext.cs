using Downroot.Content.Registries;
using Downroot.Core.Ids;
using Downroot.Core.World;
using Downroot.World.Models;

namespace Downroot.World.Generation;

public sealed class WorldGenContext(WorldModel world, ContentRegistrySet registries, IList<WorldSpawnDef> spawns) : IWorldGenContext
{
    public int Width => world.Surface.Width;
    public int Height => world.Surface.Height;

    public bool HasTerrain(ContentId contentId) => registries.Terrains.TryGet(contentId, out _);

    public ContentId? GetBaseTerrain(TileCoord coord) => world.Surface.GetBaseTerrainId(coord.X, coord.Y);

    public ContentId? GetTerrain(TileCoord coord) => world.Surface.GetTerrainId(coord.X, coord.Y);

    public void SetBaseTerrain(TileCoord coord, ContentId terrainId) => world.Surface.SetBaseTerrain(coord.X, coord.Y, terrainId);

    public void SetCoverTerrain(TileCoord coord, ContentId? terrainId) => world.Surface.SetCoverTerrain(coord.X, coord.Y, terrainId);

    public string GetSurfaceRegion(TileCoord coord) => world.Surface.GetSurfaceRegion(coord.X, coord.Y);

    public bool HasSurfaceRegion(TileCoord coord, string regionKey) => world.Surface.HasSurfaceRegion(coord.X, coord.Y, regionKey);

    public void SetSurfaceRegion(TileCoord coord, string regionKey) => world.Surface.SetSurfaceRegion(coord.X, coord.Y, regionKey);

    public bool IsSpawnOccupied(TileCoord coord) => spawns.Any(spawn => spawn.Tile == coord);

    public void AddSpawn(TileCoord coord, ContentId contentId)
    {
        if (IsSpawnOccupied(coord))
        {
            return;
        }

        spawns.Add(new WorldSpawnDef(contentId, coord));
    }
}
