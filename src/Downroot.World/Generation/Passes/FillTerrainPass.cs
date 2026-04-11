using Downroot.Core.World;

namespace Downroot.World.Generation.Passes;

public sealed class FillTerrainPass(string terrainId) : IWorldGenPass
{
    public string Name => "fill-terrain";

    public void Execute(IWorldGenContext context)
    {
        var terrainIdValue = context.GetTerrainId(terrainId);

        for (var y = 0; y < context.Height; y++)
        {
            for (var x = 0; x < context.Width; x++)
            {
                context.SetTerrain(new TileCoord(x, y), terrainIdValue);
            }
        }
    }
}
