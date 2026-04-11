using Downroot.Core.World;

namespace Downroot.World.Generation.Passes;

public sealed class DirtPatchPass(string terrainId) : IWorldGenPass
{
    public string Name => "dirt-patch";

    public void Execute(IWorldGenContext context)
    {
        var terrain = context.GetTerrain(terrainId);

        for (var y = 0; y < context.Height; y++)
        {
            for (var x = 0; x < context.Width; x++)
            {
                if ((x + y) % 7 == 0 || (x * 3 + y) % 11 == 0)
                {
                    context.SetTerrain(new TileCoord(x, y), terrain);
                }
            }
        }
    }
}
