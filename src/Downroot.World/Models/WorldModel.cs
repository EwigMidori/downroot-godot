using Downroot.Core.World;

namespace Downroot.World.Models;

public sealed class WorldModel(
    string stableId,
    WorldSpaceKind worldSpaceKind,
    int worldSeed,
    ChunkCoord? minChunkCoord = null,
    ChunkCoord? maxChunkCoord = null,
    ChunkCoord? sourcePortalChunk = null)
{
    public string StableId { get; } = stableId;
    public WorldSpaceKind WorldSpaceKind { get; } = worldSpaceKind;
    public int WorldSeed { get; } = worldSeed;
    public ChunkCoord? MinChunkCoord { get; } = minChunkCoord;
    public ChunkCoord? MaxChunkCoord { get; } = maxChunkCoord;
    public ChunkCoord? SourcePortalChunk { get; } = sourcePortalChunk;

    public bool ContainsChunk(ChunkCoord coord)
    {
        if (MinChunkCoord is { } min && (coord.X < min.X || coord.Y < min.Y))
        {
            return false;
        }

        if (MaxChunkCoord is { } max && (coord.X > max.X || coord.Y > max.Y))
        {
            return false;
        }

        return true;
    }
}
