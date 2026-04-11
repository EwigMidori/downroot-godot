using Downroot.Core.World;
using Downroot.World.Generation.Passes;

namespace Downroot.World.Generation;

public static class WorldGenPassFactory
{
    public static IWorldGenPass Create(WorldGenPassDef definition)
    {
        return definition.PassType switch
        {
            "fill-terrain" => new FillTerrainPass(definition.TerrainId),
            "dirt-patch" => new DirtPatchPass(definition.TerrainId),
            _ => throw new InvalidOperationException($"Unknown world gen pass type '{definition.PassType}' for '{definition.Key}'.")
        };
    }
}
