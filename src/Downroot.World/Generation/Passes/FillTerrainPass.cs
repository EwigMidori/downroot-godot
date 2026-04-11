using Downroot.Core.Ids;
using Downroot.Core.World;

namespace Downroot.World.Generation.Passes;

public sealed class FillTerrainPass(ContentId terrainId) : IWorldGenPass
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
                context.SetTerrain(new TileCoord(x, y), terrainId);
            }
        }
    }
}
