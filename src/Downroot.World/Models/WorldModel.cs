namespace Downroot.World.Models;

public sealed class WorldModel(ChunkData surface)
{
    public ChunkData Surface { get; } = surface;
}
