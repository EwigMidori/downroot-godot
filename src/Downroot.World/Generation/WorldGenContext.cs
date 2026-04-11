using Downroot.Content.Registries;
using Downroot.Core.Ids;
using Downroot.Core.World;
using Downroot.World.Models;

namespace Downroot.World.Generation;

public sealed class WorldGenContext(WorldModel world, ContentRegistrySet registries) : IWorldGenContext
{
    public int Width => world.Surface.Width;
    public int Height => world.Surface.Height;

    public bool HasTerrain(ContentId contentId) => registries.Terrains.TryGet(contentId, out _);

    public void SetTerrain(TileCoord coord, ContentId terrainId) => world.Surface.SetTerrain(coord.X, coord.Y, terrainId);
}
