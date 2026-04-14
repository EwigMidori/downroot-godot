using Downroot.Core.World;

namespace Downroot.World.Models;

public sealed class GeneratedChunk(
    WorldSpaceKind worldSpaceKind,
    ChunkCoord coord,
    ChunkData surface,
    IReadOnlyList<WorldSpawnDef> naturalSpawns)
{
    public WorldSpaceKind WorldSpaceKind { get; } = worldSpaceKind;
    public ChunkCoord Coord { get; } = coord;
    public ChunkData Surface { get; } = surface;
    public IReadOnlyList<WorldSpawnDef> NaturalSpawns { get; } = naturalSpawns;
}
