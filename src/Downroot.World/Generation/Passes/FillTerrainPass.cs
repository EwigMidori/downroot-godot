using Downroot.Core.World;
using Downroot.Core.Ids;

namespace Downroot.World.Generation.Passes;

public sealed class FillTerrainPass(ContentId terrainId, string surfaceRegion) : IWorldGenPass
{
    public string Name => "fill-terrain";

    public void Execute(IWorldGenContext context)
    {
        if (!context.HasTerrain(terrainId))
        {
            throw new InvalidOperationException($"Missing terrain '{terrainId}' for fill pass.");
        }

        for (var y = 0; y < context.Height; y++)
        {
            for (var x = 0; x < context.Width; x++)
            {
                var coord = new TileCoord(x, y);
                context.SetBaseTerrain(coord, terrainId);
                context.SetCoverTerrain(coord, null);
                context.SetSurfaceRegion(coord, surfaceRegion);
            }
        }
    }
}
