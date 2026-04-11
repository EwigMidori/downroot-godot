using Downroot.Content.Registries;
using Downroot.Core.World;
using Downroot.World.Models;

namespace Downroot.World.Generation;

public sealed class WorldGenerator(ContentRegistrySet registries, IReadOnlyList<IWorldGenPass> passes)
{
    public WorldModel Generate(int width, int height)
    {
        var world = new WorldModel(new ChunkData(width, height));
        var context = new WorldGenContext(world, registries);

        foreach (var pass in passes)
        {
            pass.Execute(context);
        }

        return world;
    }
}
