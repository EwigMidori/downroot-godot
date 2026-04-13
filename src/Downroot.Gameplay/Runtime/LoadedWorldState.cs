using Downroot.Core.Ids;
using Downroot.Core.World;
using Downroot.World.Models;

namespace Downroot.Gameplay.Runtime;

public sealed class LoadedWorldState
{
    private readonly Dictionary<ChunkCoord, ChunkRuntimeState> _loadedChunks = [];
    private readonly Dictionary<ChunkCoord, ChunkRuntimeState> _archivedChunks = [];

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

    public bool ContainsChunk(ChunkCoord coord) => Model.ContainsChunk(coord);

    public IEnumerable<WorldEntityState> EnumerateEntities() => _loadedChunks.Values.SelectMany(chunk => chunk.Entities);

    public bool TryGetChunk(ChunkCoord coord, out ChunkRuntimeState chunk) => _loadedChunks.TryGetValue(coord, out chunk!);

    public void LoadChunk(GeneratedChunk generatedChunk, Func<GeneratedChunk, ChunkRuntimeState> initializeChunk)
    {
        if (_loadedChunks.ContainsKey(generatedChunk.Coord))
        {
            return;
        }

        if (_archivedChunks.Remove(generatedChunk.Coord, out var archived))
        {
            _loadedChunks.Add(generatedChunk.Coord, archived);
            return;
        }

        _loadedChunks.Add(generatedChunk.Coord, initializeChunk(generatedChunk));
    }

    public void UnloadChunk(ChunkCoord coord)
    {
        if (_loadedChunks.Remove(coord, out var chunk))
        {
            _archivedChunks[coord] = chunk.CreateArchiveCopy();
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
}
