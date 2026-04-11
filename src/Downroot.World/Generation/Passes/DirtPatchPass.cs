using Downroot.Core.Ids;
using Downroot.Core.World;

namespace Downroot.World.Generation.Passes;

public sealed class DirtPatchPass(ContentId terrainId) : IWorldGenPass
{
    public string Name => "dirt-patch";

    public void Execute(IWorldGenContext context)
    {
        if (!context.HasTerrain(terrainId))
        {
            throw new InvalidOperationException($"Missing terrain '{terrainId}' for dirt patch pass.");
        }

        for (var y = 0; y < context.Height; y++)
        {
            for (var x = 0; x < context.Width; x++)
            {
                if ((x + y) % 7 == 0 || (x * 3 + y) % 11 == 0)
                {
                    context.SetTerrain(new TileCoord(x, y), terrainId);
                }
            }
        }
    }
}
