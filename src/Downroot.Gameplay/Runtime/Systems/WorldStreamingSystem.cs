using Downroot.Core.World;
using Downroot.Gameplay.Bootstrap;

namespace Downroot.Gameplay.Runtime.Systems;

public sealed class WorldStreamingSystem(GameRuntime runtime, WorldRuntimeFacade worldFacade)
{
    public void UpdateLoadedChunks()
    {
        var world = worldFacade.GetActiveWorld();
        var centerChunk = worldFacade.GetChunkCoord(runtime.Player.Position);
        var desired = new HashSet<ChunkCoord>();

        for (var y = centerChunk.Y - world.LoadRadius; y <= centerChunk.Y + world.LoadRadius; y++)
        {
            for (var x = centerChunk.X - world.LoadRadius; x <= centerChunk.X + world.LoadRadius; x++)
            {
                var coord = new ChunkCoord(x, y);
                if (!world.ContainsChunk(coord))
                {
                    continue;
                }

                desired.Add(coord);
                if (world.LoadedChunks.ContainsKey(coord))
                {
                    continue;
                }

                var generated = worldFacade.GetGenerator(world.WorldSpaceKind)
                    .GenerateChunk(world.WorldSpaceKind, world.WorldSeed, coord, runtime.ChunkWidth, runtime.ChunkHeight);
                world.LoadChunk(generated, chunk => GameBootstrapper.CreateChunkRuntimeState(runtime, chunk));
            }
        }

        foreach (var staleChunk in world.LoadedChunks.Keys.Where(coord => !desired.Contains(coord)).ToArray())
        {
            world.UnloadChunk(staleChunk);
        }

        worldFacade.RefreshEntityProjection();
    }

    public void UpdateLoadedChunksForWorld(LoadedWorldState world, WorldTileCoord aroundTile)
    {
        var currentWorld = runtime.ActiveWorldSpaceKind;
        var savedPosition = runtime.Player.Position;
        runtime.ActiveWorldSpaceKind = world.WorldSpaceKind;
        runtime.Player.Position = worldFacade.GetWorldPosition(aroundTile);
        UpdateLoadedChunks();
        runtime.Player.Position = savedPosition;
        runtime.ActiveWorldSpaceKind = currentWorld;
        worldFacade.RefreshEntityProjection();
    }

    public void ReassignRuntimeEntities()
    {
        var world = worldFacade.GetActiveWorld();
        foreach (var sourceChunk in world.LoadedChunks.Values.ToArray())
        {
            foreach (var entity in sourceChunk.RuntimeEntities.Values.Where(entity => !entity.Removed).ToArray())
            {
                var targetChunk = worldFacade.GetChunkCoord(entity.Position);
                if (targetChunk == entity.ChunkCoord || !world.ContainsChunk(targetChunk) || !world.LoadedChunks.ContainsKey(targetChunk))
                {
                    continue;
                }

                if (!sourceChunk.TakeRuntimeEntity(entity.Id, out _))
                {
                    continue;
                }

                entity.ChunkCoord = targetChunk;
                world.LoadedChunks[targetChunk].AddRuntimeEntity(entity);
            }
        }
    }
}
