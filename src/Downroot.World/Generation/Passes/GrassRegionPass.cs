using Downroot.Core.Ids;
using Downroot.Core.World;

namespace Downroot.World.Generation.Passes;

public sealed class GrassRegionPass(ContentId terrainId) : IWorldGenPass
{
    private const float BaseFrequency = 0.16f;
    private const float DetailFrequency = 0.34f;
    private const float GrassThreshold = 0.57f;

    public string Name => "grass-region";

    public void Execute(IWorldGenContext context)
    {
        if (!context.HasTerrain(terrainId))
        {
            throw new InvalidOperationException($"Missing terrain '{terrainId}' for grass region pass.");
        }

        for (var y = 0; y < context.Height; y++)
        {
            for (var x = 0; x < context.Width; x++)
            {
                var value = SampleLayeredNoise(x, y);
                var coord = new TileCoord(x, y);
                if (value >= GrassThreshold)
                {
                    context.SetCoverTerrain(coord, terrainId);
                    context.SetSurfaceRegion(coord, SurfaceRegions.GrassField);
                }
                else
                {
                    context.SetCoverTerrain(coord, null);
                    if (context.GetBaseTerrain(coord) is not null)
                    {
                        context.SetSurfaceRegion(coord, SurfaceRegions.DirtField);
                    }
                }
            }
        }
    }

    private static float SampleLayeredNoise(int x, int y)
    {
        var baseNoise = SampleValueNoise(x * BaseFrequency, y * BaseFrequency, 17);
        var detailNoise = SampleValueNoise(x * DetailFrequency, y * DetailFrequency, 79);
        return baseNoise * 0.75f + detailNoise * 0.25f;
    }

    private static float SampleValueNoise(float x, float y, int seed)
    {
        var x0 = (int)MathF.Floor(x);
        var y0 = (int)MathF.Floor(y);
        var x1 = x0 + 1;
        var y1 = y0 + 1;

        var tx = SmoothStep(x - x0);
        var ty = SmoothStep(y - y0);

        var v00 = HashToUnitFloat(x0, y0, seed);
        var v10 = HashToUnitFloat(x1, y0, seed);
        var v01 = HashToUnitFloat(x0, y1, seed);
        var v11 = HashToUnitFloat(x1, y1, seed);

        var top = Lerp(v00, v10, tx);
        var bottom = Lerp(v01, v11, tx);
        return Lerp(top, bottom, ty);
    }

    private static float HashToUnitFloat(int x, int y, int seed)
    {
        var hash = x * 374761393 + y * 668265263 + seed * 69069;
        hash = (hash ^ (hash >> 13)) * 1274126177;
        hash ^= hash >> 16;
        var positive = hash & 0x7fffffff;
        return positive / (float)int.MaxValue;
    }

    private static float SmoothStep(float value) => value * value * (3f - 2f * value);

    private static float Lerp(float a, float b, float t) => a + ((b - a) * t);
}
