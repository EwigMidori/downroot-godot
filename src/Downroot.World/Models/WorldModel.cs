using Downroot.Core.World;

namespace Downroot.World.Models;

public sealed class WorldModel(ChunkData surface, IReadOnlyList<WorldSpawnDef> spawns)
{
    public ChunkData Surface { get; } = surface;
    public IReadOnlyList<WorldSpawnDef> Spawns { get; } = spawns;
}
