using Downroot.Core.World;

namespace Downroot.Gameplay.Runtime;

public sealed class LightingField
{
    private readonly LightingFieldCell[] _cells;

    public LightingField(int minTileX, int minTileY, int width, int height, float outdoorSkylightLevel, float indoorSkylightLevel)
    {
        MinTileX = minTileX;
        MinTileY = minTileY;
        Width = Math.Max(1, width);
        Height = Math.Max(1, height);
        OutdoorSkylightLevel = outdoorSkylightLevel;
        IndoorSkylightLevel = indoorSkylightLevel;
        _cells = new LightingFieldCell[Width * Height];
    }

    public int MinTileX { get; }
    public int MinTileY { get; }
    public int Width { get; }
    public int Height { get; }
    public float OutdoorSkylightLevel { get; }
    public float IndoorSkylightLevel { get; }

    public LightingField CloneWithLevels(float outdoorSkylightLevel, float indoorSkylightLevel)
    {
        var clone = new LightingField(MinTileX, MinTileY, Width, Height, outdoorSkylightLevel, indoorSkylightLevel);
        Array.Copy(_cells, clone._cells, _cells.Length);
        return clone;
    }

    public void SetCell(WorldTileCoord tile, float localLightLevel, float skylightLevel)
    {
        if (!TryGetIndex(tile, out var index))
        {
            return;
        }

        var clampedLocal = Math.Clamp(localLightLevel, 0f, 1f);
        var clampedSkylight = Math.Clamp(skylightLevel, 0f, 1f);
        _cells[index] = new LightingFieldCell(
            clampedLocal,
            clampedSkylight,
            Math.Max(clampedLocal, clampedSkylight));
    }

    public LightingFieldCell Sample(WorldTileCoord tile)
    {
        return TryGetIndex(tile, out var index)
            ? _cells[index]
            : new LightingFieldCell(0f, OutdoorSkylightLevel, OutdoorSkylightLevel);
    }

    public float SampleCombined(WorldTileCoord tile) => Sample(tile).CombinedLightLevel;

    private bool TryGetIndex(WorldTileCoord tile, out int index)
    {
        var x = tile.X - MinTileX;
        var y = tile.Y - MinTileY;
        if (x < 0 || y < 0 || x >= Width || y >= Height)
        {
            index = -1;
            return false;
        }

        index = (y * Width) + x;
        return true;
    }
}

public readonly record struct LightingFieldCell(
    float LocalLightLevel,
    float SkylightLevel,
    float CombinedLightLevel);
