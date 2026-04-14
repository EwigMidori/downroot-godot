using System.Numerics;
using Downroot.Core.Ids;
using Downroot.Core.World;
using Downroot.World.Generation;

namespace Downroot.Gameplay.Runtime;

public sealed class WorldRuntimeFacade(GameRuntime runtime)
{
    public LoadedWorldState GetActiveWorld() => runtime.WorldState.GetActiveWorld();

    public LoadedWorldState GetWorld(WorldSpaceKind worldSpaceKind) => runtime.GetWorld(worldSpaceKind);

    public WorldGenerator GetGenerator(WorldSpaceKind worldSpaceKind) => runtime.GetWorldGenerator(worldSpaceKind);

    public ChunkCoord GetChunkCoord(Vector2 worldPosition) => runtime.GetChunkCoord(worldPosition);

    public WorldTileCoord GetWorldTile(Vector2 worldPosition) => runtime.GetWorldTile(worldPosition);

    public Vector2 GetWorldPosition(WorldTileCoord tileCoord) => runtime.GetWorldPosition(tileCoord);

    public bool TryGetChunk(WorldSpaceKind worldSpaceKind, ChunkCoord coord, out ChunkRuntimeState chunk)
    {
        return GetWorld(worldSpaceKind).TryGetChunk(coord, out chunk);
    }

    public bool TryGetChunkForTile(WorldSpaceKind worldSpaceKind, WorldTileCoord tile, out ChunkRuntimeState chunk, out LocalTileCoord localCoord)
    {
        return GetWorld(worldSpaceKind).TryGetChunkForTile(tile, runtime.ChunkWidth, runtime.ChunkHeight, out chunk, out localCoord);
    }

    public ContentId? GetRaisedFeatureId(WorldSpaceKind worldSpaceKind, WorldTileCoord tile)
    {
        return GetWorld(worldSpaceKind).GetRaisedFeatureId(tile, runtime.ChunkWidth, runtime.ChunkHeight);
    }

    public byte GetRaisedFeatureVariantIndex(WorldSpaceKind worldSpaceKind, WorldTileCoord tile)
    {
        return GetWorld(worldSpaceKind).GetRaisedFeatureVariantIndex(tile, runtime.ChunkWidth, runtime.ChunkHeight);
    }

    public void RemoveRaisedFeature(WorldSpaceKind worldSpaceKind, WorldTileCoord tile)
    {
        GetWorld(worldSpaceKind).RemoveRaisedFeature(tile, runtime.ChunkWidth, runtime.ChunkHeight);
    }

    public void AddRuntimeEntity(WorldSpaceKind worldSpaceKind, WorldEntityState entity)
    {
        GetWorld(worldSpaceKind).AddRuntimeEntity(entity);
    }

    public void RefreshEntityProjection()
    {
        runtime.WorldState.RefreshEntityProjection();
    }

    public void RemoveDeleted()
    {
        runtime.WorldState.RemoveDeleted();
    }
}
