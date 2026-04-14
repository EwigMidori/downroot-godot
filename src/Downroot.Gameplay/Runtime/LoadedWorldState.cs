using Downroot.Core.Ids;
using Downroot.Core.World;
using Downroot.World.Models;

namespace Downroot.Gameplay.Runtime;

public sealed class LoadedWorldState
{
    private readonly Dictionary<ChunkCoord, ChunkRuntimeState> _loadedChunks = [];
    private readonly Dictionary<ChunkCoord, ChunkRuntimeArchive> _archivedChunks = [];

    public LoadedWorldState(WorldModel model, int loadRadius)
    {
        Model = model;
        LoadRadius = loadRadius;
    }

    public WorldModel Model { get; }
    public WorldSpaceKind WorldSpaceKind => Model.WorldSpaceKind;
    public int WorldSeed => Model.WorldSeed;
    public int LoadRadius { get; }
    public Dictionary<ChunkCoord, ChunkRuntimeState> LoadedChunks => _loadedChunks;
    public HashSet<WorldTileCoord> DirtyRaisedFeatureTiles { get; } = [];

    public bool ContainsChunk(ChunkCoord coord) => Model.ContainsChunk(coord);

    public IEnumerable<WorldEntityState> EnumerateEntities() => _loadedChunks.Values.SelectMany(chunk => chunk.Entities);

    public bool TryGetChunk(ChunkCoord coord, out ChunkRuntimeState chunk) => _loadedChunks.TryGetValue(coord, out chunk!);

    public bool TryGetChunkForTile(WorldTileCoord tile, int chunkWidth, int chunkHeight, out ChunkRuntimeState chunk, out LocalTileCoord localCoord)
    {
        var chunkCoord = tile.ToChunkCoord(chunkWidth, chunkHeight);
        if (_loadedChunks.TryGetValue(chunkCoord, out chunk!))
        {
            localCoord = tile.ToLocalCoord(chunkWidth, chunkHeight);
            return true;
        }

        localCoord = default;
        return false;
    }

    public bool HasRaisedFeature(WorldTileCoord tile, int chunkWidth, int chunkHeight)
    {
        if (!TryGetChunkForTile(tile, chunkWidth, chunkHeight, out var chunk, out var localCoord))
        {
            return false;
        }

        return chunk.GeneratedChunk.Surface.GetRaisedFeatureId(localCoord.X, localCoord.Y) is not null
            && !chunk.RemovedRaisedFeatureTiles.Contains(tile);
    }

    public ContentId? GetRaisedFeatureId(WorldTileCoord tile, int chunkWidth, int chunkHeight)
    {
        if (!TryGetChunkForTile(tile, chunkWidth, chunkHeight, out var chunk, out var localCoord))
        {
            return null;
        }

        return chunk.RemovedRaisedFeatureTiles.Contains(tile)
            ? null
            : chunk.GeneratedChunk.Surface.GetRaisedFeatureId(localCoord.X, localCoord.Y);
    }

    public byte GetRaisedFeatureVariantIndex(WorldTileCoord tile, int chunkWidth, int chunkHeight)
    {
        if (!TryGetChunkForTile(tile, chunkWidth, chunkHeight, out var chunk, out var localCoord))
        {
            return 0;
        }

        var featureId = GetRaisedFeatureId(tile, chunkWidth, chunkHeight);
        if (featureId is null)
        {
            return 0;
        }

        if (!HasRemovedRaisedFeatureInNeighborhood(tile, chunkWidth, chunkHeight))
        {
            return chunk.GeneratedChunk.Surface.GetRaisedFeatureVariantIndex(localCoord.X, localCoord.Y);
        }

        return (byte)Downroot.World.Generation.RaisedFeatureAutotileResolver.Resolve(
            IsSameRaisedFeature(tile, new WorldTileCoord(tile.X, tile.Y - 1), featureId.Value, chunkWidth, chunkHeight),
            IsSameRaisedFeature(tile, new WorldTileCoord(tile.X + 1, tile.Y), featureId.Value, chunkWidth, chunkHeight),
            IsSameRaisedFeature(tile, new WorldTileCoord(tile.X, tile.Y + 1), featureId.Value, chunkWidth, chunkHeight),
            IsSameRaisedFeature(tile, new WorldTileCoord(tile.X - 1, tile.Y), featureId.Value, chunkWidth, chunkHeight),
            IsSameRaisedFeature(tile, new WorldTileCoord(tile.X + 1, tile.Y - 1), featureId.Value, chunkWidth, chunkHeight),
            IsSameRaisedFeature(tile, new WorldTileCoord(tile.X - 1, tile.Y - 1), featureId.Value, chunkWidth, chunkHeight),
            IsSameRaisedFeature(tile, new WorldTileCoord(tile.X + 1, tile.Y + 1), featureId.Value, chunkWidth, chunkHeight),
            IsSameRaisedFeature(tile, new WorldTileCoord(tile.X - 1, tile.Y + 1), featureId.Value, chunkWidth, chunkHeight));
    }

    public void MarkRaisedFeatureDirty(IEnumerable<WorldTileCoord> tiles)
    {
        foreach (var tile in tiles)
        {
            DirtyRaisedFeatureTiles.Add(tile);
        }
    }

    public WorldTileCoord[] ConsumeDirtyRaisedFeatureTiles()
    {
        var dirty = DirtyRaisedFeatureTiles.ToArray();
        DirtyRaisedFeatureTiles.Clear();
        return dirty;
    }

    public void LoadChunk(GeneratedChunk generatedChunk, Func<GeneratedChunk, ChunkRuntimeState> initializeChunk)
    {
        if (!ContainsChunk(generatedChunk.Coord))
        {
            throw new InvalidOperationException($"Chunk {generatedChunk.Coord} is outside the bounds of world '{Model.StableId}'.");
        }

        if (generatedChunk.WorldSpaceKind != WorldSpaceKind)
        {
            throw new InvalidOperationException($"Chunk {generatedChunk.Coord} belongs to {generatedChunk.WorldSpaceKind}, but this container is {WorldSpaceKind}.");
        }

        if (_loadedChunks.ContainsKey(generatedChunk.Coord))
        {
            return;
        }

        var chunk = initializeChunk(generatedChunk);
        if (_archivedChunks.Remove(generatedChunk.Coord, out var archived))
        {
            chunk.ApplyArchive(archived);
        }

        _loadedChunks.Add(generatedChunk.Coord, chunk);
    }

    public void UnloadChunk(ChunkCoord coord)
    {
        if (_loadedChunks.Remove(coord, out var chunk))
        {
            _archivedChunks[coord] = chunk.CreateArchive();
        }
    }

    public void AddRuntimeEntity(WorldEntityState entity)
    {
        if (!_loadedChunks.TryGetValue(entity.ChunkCoord, out var chunk))
        {
            throw new InvalidOperationException($"Chunk {entity.ChunkCoord} is not loaded in world space {WorldSpaceKind}.");
        }

        chunk.AddRuntimeEntity(entity);
    }

    public bool TryFindEntity(EntityId entityId, out WorldEntityState? entity, out ChunkRuntimeState? chunk)
    {
        foreach (var loadedChunk in _loadedChunks.Values)
        {
            entity = loadedChunk.Entities.FirstOrDefault(candidate => candidate.Id == entityId);
            if (entity is not null)
            {
                chunk = loadedChunk;
                return true;
            }
        }

        entity = null;
        chunk = null;
        return false;
    }

    private bool IsSameRaisedFeature(WorldTileCoord originTile, WorldTileCoord neighborTile, ContentId featureId, int chunkWidth, int chunkHeight)
    {
        if (!TryGetChunkForTile(neighborTile, chunkWidth, chunkHeight, out var chunk, out var localCoord))
        {
            var originChunk = originTile.ToChunkCoord(chunkWidth, chunkHeight);
            var neighborChunk = neighborTile.ToChunkCoord(chunkWidth, chunkHeight);
            if (!ContainsChunk(neighborChunk) || !_archivedChunks.TryGetValue(neighborChunk, out var archived))
            {
                return false;
            }

            var removed = archived.RemovedRaisedFeatureTiles.Contains(neighborTile);
            if (removed)
            {
                return false;
            }

            return false;
        }

        if (chunk.RemovedRaisedFeatureTiles.Contains(neighborTile))
        {
            return false;
        }

        return chunk.GeneratedChunk.Surface.GetRaisedFeatureId(localCoord.X, localCoord.Y) == featureId;
    }

    private bool HasRemovedRaisedFeatureInNeighborhood(WorldTileCoord tile, int chunkWidth, int chunkHeight)
    {
        for (var dy = -1; dy <= 1; dy++)
        {
            for (var dx = -1; dx <= 1; dx++)
            {
                var sample = new WorldTileCoord(tile.X + dx, tile.Y + dy);
                var chunkCoord = sample.ToChunkCoord(chunkWidth, chunkHeight);
                if (_loadedChunks.TryGetValue(chunkCoord, out var chunk) && chunk.RemovedRaisedFeatureTiles.Contains(sample))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
