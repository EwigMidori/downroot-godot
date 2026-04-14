using Downroot.Core.Ids;
using Downroot.Core.World;

namespace Downroot.World.Generation.Passes;

public sealed class RiverPass(ContentId riverTerrainId) : IWorldGenPass
{
    public string Name => WorldGenPassTypes.River;

    public void Execute(IWorldGenContext context)
    {
        if (!context.HasTerrain(riverTerrainId))
        {
            throw new InvalidOperationException($"Missing terrain '{riverTerrainId}' for river pass.");
        }

        for (var y = 0; y < context.Height; y++)
        {
            for (var x = 0; x < context.Width; x++)
            {
                var local = new LocalTileCoord(x, y);
                var world = context.GetWorldTileCoord(local);
                if (!IsRiverTile(context, world))
                {
                    continue;
                }

                context.SetCoverTerrain(local, riverTerrainId);
                context.SetSurfaceRegion(local, SurfaceRegions.River);
            }
        }
    }

    public static bool IsRiverTile(IWorldGenContext context, WorldTileCoord world)
    {
        var primaryCenter = (MathF.Sin((world.X * 0.085f) + (context.WorldSeed * 0.013f)) * 4.25f)
            + (context.GetStableUnitValue(new WorldTileCoord(world.X / 6, 0), 991) * 6f)
            - 3f;
        var secondaryCenter = (MathF.Cos((world.X * 0.048f) - (context.WorldSeed * 0.009f)) * 6.5f)
            + 22f;
        var primaryWidth = 1.8f + (context.GetStableUnitValue(new WorldTileCoord(world.X / 4, 1), 1441) * 0.9f);
        var secondaryWidth = 1.4f + (context.GetStableUnitValue(new WorldTileCoord(world.X / 5, 2), 2111) * 0.7f);
        return MathF.Abs(world.Y - primaryCenter) <= primaryWidth
            || MathF.Abs(world.Y - secondaryCenter) <= secondaryWidth;
    }
}
