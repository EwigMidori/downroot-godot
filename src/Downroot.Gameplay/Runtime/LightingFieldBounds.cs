using Downroot.Core.World;

namespace Downroot.Gameplay.Runtime;

public readonly record struct LightingFieldBounds(int MinTileX, int MinTileY, int Width, int Height)
{
    public int MaxTileX => MinTileX + Width - 1;
    public int MaxTileY => MinTileY + Height - 1;

    public bool IsEmpty => Width <= 0 || Height <= 0;

    public bool Contains(WorldTileCoord tile)
    {
        return tile.X >= MinTileX
            && tile.X <= MaxTileX
            && tile.Y >= MinTileY
            && tile.Y <= MaxTileY;
    }

    public bool Intersects(LightingFieldBounds other)
    {
        return !(other.MaxTileX < MinTileX
            || other.MinTileX > MaxTileX
            || other.MaxTileY < MinTileY
            || other.MinTileY > MaxTileY);
    }

    public LightingFieldBounds Expand(int tiles)
    {
        return new LightingFieldBounds(
            MinTileX - tiles,
            MinTileY - tiles,
            Width + (tiles * 2),
            Height + (tiles * 2));
    }

    public LightingFieldBounds ClampTo(LightingFieldBounds outer)
    {
        var minX = Math.Max(MinTileX, outer.MinTileX);
        var minY = Math.Max(MinTileY, outer.MinTileY);
        var maxX = Math.Min(MaxTileX, outer.MaxTileX);
        var maxY = Math.Min(MaxTileY, outer.MaxTileY);
        if (maxX < minX || maxY < minY)
        {
            return new LightingFieldBounds(outer.MinTileX, outer.MinTileY, 0, 0);
        }

        return new LightingFieldBounds(minX, minY, (maxX - minX) + 1, (maxY - minY) + 1);
    }

    public static LightingFieldBounds FromTile(WorldTileCoord tile)
    {
        return new LightingFieldBounds(tile.X, tile.Y, 1, 1);
    }

    public static LightingFieldBounds Union(LightingFieldBounds a, LightingFieldBounds b)
    {
        if (a.IsEmpty)
        {
            return b;
        }

        if (b.IsEmpty)
        {
            return a;
        }

        var minX = Math.Min(a.MinTileX, b.MinTileX);
        var minY = Math.Min(a.MinTileY, b.MinTileY);
        var maxX = Math.Max(a.MaxTileX, b.MaxTileX);
        var maxY = Math.Max(a.MaxTileY, b.MaxTileY);
        return new LightingFieldBounds(minX, minY, (maxX - minX) + 1, (maxY - minY) + 1);
    }
}
