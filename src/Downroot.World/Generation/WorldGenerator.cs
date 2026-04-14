using Downroot.Content.Registries;
using Downroot.Core.World;
using Downroot.World.Models;

namespace Downroot.World.Generation;

public sealed class WorldGenerator(ContentRegistrySet registries, IReadOnlyList<IWorldGenPass> passes)
{
    public GeneratedChunk GenerateChunk(WorldSpaceKind worldSpaceKind, int worldSeed, ChunkCoord chunkCoord, int width, int height)
    {
        var spawns = new List<WorldSpawnDef>();
        var surface = new ChunkData(width, height);
        var context = new WorldGenContext(worldSpaceKind, worldSeed, chunkCoord, surface, registries, spawns);

        foreach (var pass in passes)
        {
            pass.Execute(context);
        }

        return new GeneratedChunk(worldSpaceKind, chunkCoord, surface, spawns.ToArray());
    }
}
