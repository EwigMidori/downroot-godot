using Downroot.Core.World;
using Downroot.World.Generation.Passes;

namespace Downroot.World.Generation;

public static class WorldGenPassFactory
{
    public static IWorldGenPass Create(WorldGenPassDef definition)
    {
        return definition.PassType switch
        {
            "fill-terrain" => new FillTerrainPass(definition.TargetId, definition.PrimarySurfaceRegion ?? SurfaceRegions.DirtField),
            "grass-region" => new GrassRegionPass(definition.TargetId),
            "dirt-patch" => new DirtPatchPass(definition.TargetId),
            "scatter-spawn" => new ScatterSpawnPass(
                definition.TargetId,
                definition.Count,
                definition.StartColumn,
                definition.StartRow,
                definition.Width,
                definition.Height,
                definition.PrimarySurfaceRegion,
                definition.MinSpacing),
            _ => throw new InvalidOperationException($"Unknown world gen pass type '{definition.PassType}' for '{definition.Id}'.")
        };
    }
}
