using Downroot.Core.Ids;
using Downroot.Core.World;

namespace Downroot.World.Generation.Passes;

public sealed class ScatterSpawnPass(ContentId targetId, int count, int startColumn, int startRow, int width, int height) : IWorldGenPass
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

        for (var index = 0; index < count; index++)
        {
            var x = originX + ((index * 5 + 3) % Math.Max(1, usableWidth));
            var y = originY + ((index * 7 + 2) % Math.Max(1, usableHeight));
            context.AddSpawn(new TileCoord(x, y), targetId);
        }
    }
}
