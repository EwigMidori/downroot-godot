using System.Numerics;
using Downroot.Core.Ids;
using Downroot.Core.World;
using Downroot.Gameplay.Bootstrap;

namespace Downroot.Gameplay.Runtime.Systems;

public sealed class PortalTravelSystem(
    GameRuntime runtime,
    WorldRuntimeFacade worldFacade,
    WorldStreamingSystem worldStreamingSystem,
    MovementSystem movementSystem)
{
    public void TickTravel(float deltaSeconds)
    {
        var travel = runtime.WorldState.Travel;
        travel.PhaseRemainingSeconds = Math.Max(0f, travel.PhaseRemainingSeconds - deltaSeconds);
        if (travel.PhaseRemainingSeconds > 0f)
        {
            return;
        }

        switch (travel.Phase)
        {
            case WorldTravelPhase.FadingOut:
                travel.Phase = WorldTravelPhase.Switching;
                travel.PhaseRemainingSeconds = 0.05f;
                PerformWorldSwitch();
                break;
            case WorldTravelPhase.Switching:
                travel.Phase = WorldTravelPhase.FadingIn;
                travel.PhaseRemainingSeconds = 0.25f;
                break;
            case WorldTravelPhase.FadingIn:
                travel.Reset();
                break;
        }
    }

    public void StartPortalTravel(WorldEntityState entity)
    {
        if (runtime.WorldState.Travel.IsActive)
        {
            return;
        }

        var link = worldFacade.GetPortalLink(entity.WorldSpaceKind, entity.ChunkCoord);
        var targetWorld = link.SourceWorldSpaceKind == runtime.ActiveWorldSpaceKind
            ? link.TargetWorldSpaceKind
            : link.SourceWorldSpaceKind;
        var targetPortalChunk = link.SourceWorldSpaceKind == targetWorld
            ? link.SourcePortalChunk
            : link.TargetPortalChunk;

        if (targetWorld == WorldSpaceKind.DimShardPocket && !runtime.DimShardPocket.LoadedChunks.ContainsKey(targetPortalChunk))
        {
            worldStreamingSystem.UpdateLoadedChunksForWorld(
                runtime.DimShardPocket,
                WorldTileCoord.FromChunkAndLocal(targetPortalChunk, new LocalTileCoord(0, 0), runtime.ChunkWidth, runtime.ChunkHeight));
        }

        var targetTile = FindPortalTile(worldFacade.GetWorld(targetWorld), targetPortalChunk);
        runtime.WorldState.Travel.SourceWorldSpaceKind = runtime.ActiveWorldSpaceKind;
        runtime.WorldState.Travel.TargetWorldSpaceKind = targetWorld;
        runtime.WorldState.Travel.SourcePortalChunk = entity.ChunkCoord;
        runtime.WorldState.Travel.SourcePortalTile = worldFacade.GetWorldTile(entity.Position);
        runtime.WorldState.Travel.TargetPortalTile = targetTile;
        runtime.WorldState.Travel.Phase = WorldTravelPhase.FadingOut;
        runtime.WorldState.Travel.PhaseRemainingSeconds = 0.25f;
        runtime.WorldState.SetStatusEvent(
            targetWorld == WorldSpaceKind.DimShardPocket
                ? new StatusEventState(StatusEventKind.EnteredPortal)
                : new StatusEventState(StatusEventKind.ReturnedThroughPortal),
            1.25f);
    }

    private void PerformWorldSwitch()
    {
        if (runtime.WorldState.Travel.TargetWorldSpaceKind == runtime.WorldState.Travel.SourceWorldSpaceKind)
        {
            throw new InvalidOperationException("Portal travel must switch to a different world space.");
        }

        runtime.ActiveWorldSpaceKind = runtime.WorldState.Travel.TargetWorldSpaceKind;
        var activeWorld = worldFacade.GetActiveWorld();
        if (runtime.ActiveWorldSpaceKind == WorldSpaceKind.DimShardPocket)
        {
            if (ReferenceEquals(activeWorld, runtime.Overworld)
                || activeWorld.Model.WorldSpaceKind != WorldSpaceKind.DimShardPocket
                || activeWorld.Model.StableId == "overworld"
                || activeWorld.WorldSeed == runtime.Overworld.WorldSeed)
            {
                throw new InvalidOperationException("DimShardPocket travel must activate the independent pocket world container.");
            }
        }

        runtime.WorldState.WorkspaceMode = CraftWorkspaceMode.Hidden;
        runtime.WorldState.ActiveStationEntityId = null;
        runtime.WorldState.ActiveStationKind = null;
        worldStreamingSystem.UpdateLoadedChunksForWorld(activeWorld, runtime.WorldState.Travel.TargetPortalTile);
        runtime.Player.Position = FindPortalLandingPosition(activeWorld, runtime.WorldState.Travel.TargetPortalTile);
        worldFacade.EnsureEntityProjectionCurrent();
        runtime.WorldState.SetStatusEvent(
            runtime.ActiveWorldSpaceKind == WorldSpaceKind.Overworld
                ? new StatusEventState(StatusEventKind.ReturnedThroughPortal)
                : new StatusEventState(StatusEventKind.EnteredPortal),
            1.5f);
    }

    private Vector2 FindPortalLandingPosition(LoadedWorldState world, WorldTileCoord portalTile)
    {
        var candidates = new[]
        {
            portalTile,
            new WorldTileCoord(portalTile.X, portalTile.Y + 1),
            new WorldTileCoord(portalTile.X + 1, portalTile.Y),
            new WorldTileCoord(portalTile.X - 1, portalTile.Y),
            new WorldTileCoord(portalTile.X, portalTile.Y - 1)
        };

        foreach (var candidate in candidates)
        {
            if (!world.ContainsChunk(candidate.ToChunkCoord(runtime.ChunkWidth, runtime.ChunkHeight)))
            {
                continue;
            }

            var position = worldFacade.GetWorldPosition(candidate);
            if (!movementSystem.IsBlocked(position))
            {
                return position;
            }
        }

        return worldFacade.GetWorldPosition(portalTile);
    }

    private WorldTileCoord FindPortalTile(LoadedWorldState world, ChunkCoord preferredChunk)
    {
        if (!world.LoadedChunks.ContainsKey(preferredChunk))
        {
            var generated = worldFacade.GetGenerator(world.WorldSpaceKind)
                .GenerateChunk(world.WorldSpaceKind, world.WorldSeed, preferredChunk, runtime.ChunkWidth, runtime.ChunkHeight);
            world.LoadChunk(generated, chunk => GameBootstrapper.CreateChunkRuntimeState(runtime, chunk));
        }

        var portal = world.LoadedChunks[preferredChunk].Entities.FirstOrDefault(worldFacade.IsPortalEntity);
        return portal is null ? new WorldTileCoord(0, 0) : worldFacade.GetWorldTile(portal.Position);
    }
}
