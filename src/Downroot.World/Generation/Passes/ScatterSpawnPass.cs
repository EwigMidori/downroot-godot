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

        var candidates = new List<TileCoord>();
        for (var y = originY; y < originY + usableHeight; y++)
        {
            for (var x = originX; x < originX + usableWidth; x++)
            {
                var coord = new TileCoord(x, y);
                if (requiredSurfaceRegion is not null && !context.HasSurfaceRegion(coord, requiredSurfaceRegion))
                {
                    continue;
                }

                candidates.Add(coord);
            }
        }

        var ordered = candidates
            .OrderBy(coord => StableHash(coord.X, coord.Y, targetId.Value.GetHashCode()))
            .ToArray();
        var chosen = new List<TileCoord>();
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

    private static int StableHash(int x, int y, int seed)
    {
        var hash = x * 73856093 ^ y * 19349663 ^ seed * 83492791;
        return hash & int.MaxValue;
    }

    private static int DistanceSquared(TileCoord a, TileCoord b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return (dx * dx) + (dy * dy);
    }
}
