using Downroot.Core.Ids;
using Downroot.Core.World;

namespace Downroot.World.Generation.Passes;

public sealed class RockOutcropPass(ContentId outcropId) : IWorldGenPass
{
    public string Name => "rock-outcrop";

    public void Execute(IWorldGenContext context)
    {
        for (var y = 0; y < context.Height; y++)
        {
            for (var x = 0; x < context.Width; x++)
            {
                var local = new LocalTileCoord(x, y);
                if (!context.HasSurfaceRegion(local, SurfaceRegions.DirtField) && context.WorldSpaceKind == WorldSpaceKind.Overworld)
                {
                    continue;
                }

                if (context.WorldSpaceKind == WorldSpaceKind.DimShardPocket && context.HasSurfaceRegion(local, SurfaceRegions.River))
                {
                    continue;
                }

                if (context.HasRaisedFeature(local))
                {
                    continue;
                }

                var world = context.GetWorldTileCoord(local);
                var density = context.GetStableUnitValue(world, 5003);
                var ridge = context.GetStableUnitValue(new WorldTileCoord(world.X / 2, world.Y / 2), 5011);
                if (density < (context.WorldSpaceKind == WorldSpaceKind.Overworld ? 0.93f : 0.965f) || ridge < 0.55f)
                {
                    continue;
                }

                if (context.IsSpawnOccupied(local))
                {
                    continue;
                }

                if (HasNearbyOutcrop(context, local))
                {
                    continue;
                }

                context.AddSpawn(local, outcropId);
            }
        }
    }

    private static bool HasNearbyOutcrop(IWorldGenContext context, LocalTileCoord local)
    {
        for (var dy = -1; dy <= 1; dy++)
        {
            for (var dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0)
                {
                    continue;
                }

                var sampleX = local.X + dx;
                var sampleY = local.Y + dy;
                if (sampleX < 0 || sampleY < 0 || sampleX >= context.Width || sampleY >= context.Height)
                {
                    continue;
                }

                if (context.IsSpawnOccupied(new LocalTileCoord(sampleX, sampleY)))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
