using Downroot.Core.Ids;
using Downroot.Core.World;

namespace Downroot.World.Generation.Passes;

public sealed class PortalSitePass(ContentId portalId) : IWorldGenPass
{
    public string Name => "portal-site";

    public void Execute(IWorldGenContext context)
    {
        if (context.WorldSpaceKind == WorldSpaceKind.Overworld && context.ChunkCoord != new ChunkCoord(1, 0))
        {
            return;
        }

        if (context.WorldSpaceKind == WorldSpaceKind.DimShardPocket && context.ChunkCoord != new ChunkCoord(0, 0))
        {
            return;
        }

        var center = new LocalTileCoord(context.Width / 2, context.Height / 2);
        var best = FindNearestUsableTile(context, center);
        if (best is null)
        {
            return;
        }

        context.AddSpawn(best.Value, portalId);
    }

    private static LocalTileCoord? FindNearestUsableTile(IWorldGenContext context, LocalTileCoord origin)
    {
        LocalTileCoord? best = null;
        var bestDistance = int.MaxValue;
        for (var y = 0; y < context.Height; y++)
        {
            for (var x = 0; x < context.Width; x++)
            {
                var local = new LocalTileCoord(x, y);
                if (context.IsSpawnOccupied(local) || context.HasSurfaceRegion(local, SurfaceRegions.River))
                {
                    continue;
                }

                var distance = DistanceSquared(origin, local);
                if (distance >= bestDistance)
                {
                    continue;
                }

                best = local;
                bestDistance = distance;
            }
        }

        return best;
    }

    private static int DistanceSquared(LocalTileCoord a, LocalTileCoord b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return (dx * dx) + (dy * dy);
    }
}
