using Downroot.Core.Ids;
using Downroot.Core.World;

namespace Downroot.World.Generation.Passes;

public sealed class ScatterSpawnPass(
    ContentId targetId,
    int count,
    int startColumn,
    int startRow,
    int width,
    int height,
    string? requiredSurfaceRegion,
    int minSpacing) : IWorldGenPass
{
    public string Name => "scatter-spawn";

    public void Execute(IWorldGenContext context)
    {
        if (count <= 0)
        {
            return;
        }

        var usableWidth = width > 0 ? Math.Min(width, context.Width) : context.Width;
        var usableHeight = height > 0 ? Math.Min(height, context.Height) : context.Height;
        var originX = Math.Clamp(startColumn, 0, Math.Max(0, context.Width - 1));
        var originY = Math.Clamp(startRow, 0, Math.Max(0, context.Height - 1));

        var candidates = new List<LocalTileCoord>();
        for (var y = originY; y < originY + usableHeight; y++)
        {
            for (var x = originX; x < originX + usableWidth; x++)
            {
                var coord = new LocalTileCoord(x, y);
                if (requiredSurfaceRegion is not null && !context.HasSurfaceRegion(coord, requiredSurfaceRegion))
                {
                    continue;
                }

                candidates.Add(coord);
            }
        }

        var ordered = candidates
            .OrderBy(coord => context.GetStableHash(context.GetWorldTileCoord(coord), targetId.Value.GetHashCode()))
            .ToArray();
        var chosen = new List<LocalTileCoord>();
        foreach (var coord in ordered)
        {
            if (chosen.Count >= count)
            {
                break;
            }

            if (context.IsSpawnOccupied(coord))
            {
                continue;
            }

            if (minSpacing > 0 && chosen.Any(existing => DistanceSquared(existing, coord) < minSpacing * minSpacing))
            {
                continue;
            }

            context.AddSpawn(coord, targetId);
            chosen.Add(coord);
        }
    }

    private static int DistanceSquared(LocalTileCoord a, LocalTileCoord b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return (dx * dx) + (dy * dy);
    }
}
