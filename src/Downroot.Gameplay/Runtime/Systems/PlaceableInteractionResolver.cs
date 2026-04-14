using System.Numerics;
using Downroot.Core.Definitions;
using Downroot.Core.Gameplay;
using Downroot.Core.Ids;

namespace Downroot.Gameplay.Runtime.Systems;

public sealed class PlaceableInteractionResolver(
    GameRuntime runtime,
    WorldRuntimeFacade worldFacade,
    WorldQueryService worldQuery)
{
    private const float StationRange = 56f;

    public void ValidateActiveInteractions()
    {
        ValidateActiveStorage();
        ValidateActiveStation();
    }

    public bool TryGetNearbyStation(CraftingStationKind stationKind, out WorldEntityState station)
    {
        station = worldQuery.FindNearbyStation(stationKind, StationRange)!;
        return station is not null;
    }

    public InteractionContext CreateInteractionContext(WorldEntityState entity)
    {
        var verb = ResolveVerb(entity);
        return new InteractionContext(entity.Id, entity.Kind, entity.DefinitionId, verb);
    }

    public void Interact(WorldEntityState entity)
    {
        var placeableDef = runtime.Content.Placeables.Get(entity.DefinitionId);
        if (placeableDef.HasBehavior(PlaceableBehaviorKind.Bed))
        {
            InteractBed(entity);
            return;
        }

        if (placeableDef.HasBehavior(PlaceableBehaviorKind.LightSource))
        {
            InteractLightSource(entity);
            return;
        }

        if (placeableDef.HasBehavior(PlaceableBehaviorKind.Storage))
        {
            ToggleStorage(entity, placeableDef);
            return;
        }

        if (placeableDef.HasBehavior(PlaceableBehaviorKind.CraftingStation))
        {
            ActivateStation(entity, placeableDef);
            return;
        }

        if (placeableDef.HasBehavior(PlaceableBehaviorKind.Door))
        {
            ToggleDoor(entity);
            return;
        }
    }

    public void ClearTransientUiState()
    {
        CloseActiveStorage();
        runtime.WorldState.ActiveStationKind = null;
        runtime.WorldState.ActiveStationEntityId = null;
        runtime.WorldState.WorkspaceMode = CraftWorkspaceMode.Hidden;
        runtime.WorldState.CurrentInteraction = null;
        runtime.WorldState.ActiveDestroyProgress = null;
    }

    private InteractionVerb ResolveVerb(WorldEntityState entity)
    {
        var placeableDef = runtime.Content.Placeables.Get(entity.DefinitionId);
        if (placeableDef.HasBehavior(PlaceableBehaviorKind.Bed))
        {
            return runtime.WorldState.IsNight(runtime.BootstrapConfig.DayLengthSeconds)
                ? InteractionVerb.Sleep
                : InteractionVerb.SetHome;
        }

        if (placeableDef.HasBehavior(PlaceableBehaviorKind.LightSource))
        {
            if (entity.PlaceableState?.FuelSecondsRemaining <= 0f)
            {
                return InteractionVerb.Use;
            }

            return entity.PlaceableState?.IsLit == true
                ? InteractionVerb.Extinguish
                : InteractionVerb.Light;
        }

        if (placeableDef.HasBehavior(PlaceableBehaviorKind.Storage))
        {
            return entity.OpenState ? InteractionVerb.Close : InteractionVerb.Open;
        }

        if (placeableDef.HasBehavior(PlaceableBehaviorKind.Door))
        {
            return entity.OpenState ? InteractionVerb.Close : InteractionVerb.Open;
        }

        return InteractionVerb.Use;
    }

    private void InteractBed(WorldEntityState entity)
    {
        SetPrimaryBed(entity);
        if (!runtime.WorldState.IsNight(runtime.BootstrapConfig.DayLengthSeconds))
        {
            runtime.WorldState.SetStatusEvent(new StatusEventState(StatusEventKind.HomeSet, entity.DefinitionId), 1.5f);
            return;
        }

        SleepUntilMorning();
        runtime.WorldState.SetStatusEvent(new StatusEventState(StatusEventKind.SleptUntilMorning, entity.DefinitionId), 1.8f);
    }

    private void InteractLightSource(WorldEntityState entity)
    {
        entity.PlaceableState ??= new PlaceableRuntimeState();
        var state = entity.PlaceableState;
        SyncFuel(entity);
        if (state.FuelSecondsRemaining <= 0f)
        {
            state.IsLit = false;
            worldFacade.NotifyEntityStateChanged(entity);
            runtime.WorldState.SetStatusEvent(new StatusEventState(StatusEventKind.LightBurnedOut, entity.DefinitionId), 1.5f);
            return;
        }

        state.IsLit = !state.IsLit;
        state.FuelLastUpdatedTotalSeconds = runtime.WorldState.TotalElapsedSeconds;
        worldFacade.NotifyEntityStateChanged(entity);
        runtime.WorldState.SetStatusEvent(
            new StatusEventState(state.IsLit ? StatusEventKind.LightLit : StatusEventKind.LightExtinguished, entity.DefinitionId),
            1.2f);
    }

    private void ToggleStorage(WorldEntityState entity, PlaceableDef def)
    {
        if (runtime.WorldState.ActiveStorageEntityId == entity.Id)
        {
            entity.OpenState = false;
            runtime.WorldState.ActiveStorageEntityId = null;
        }
        else
        {
            CloseActiveStorage();
            entity.OpenState = true;
            runtime.WorldState.ActiveStorageEntityId = entity.Id;
            runtime.WorldState.WorkspaceMode = CraftWorkspaceMode.Hidden;
            runtime.WorldState.ActiveStationEntityId = null;
            runtime.WorldState.ActiveStationKind = null;
            entity.StorageInventory ??= new InventoryState(def.StorageSlotCount);
        }

        worldFacade.NotifyEntityStateChanged(entity);
    }

    private void ActivateStation(WorldEntityState entity, PlaceableDef def)
    {
        CloseActiveStorage();
        runtime.WorldState.ActiveStationKind = def.CraftingStationKind;
        runtime.WorldState.ActiveStationEntityId = entity.Id;
        runtime.WorldState.WorkspaceMode = def.CraftingStationKind == CraftingStationKind.Furnace
            ? CraftWorkspaceMode.Furnace
            : CraftWorkspaceMode.Workbench;
    }

    private void ToggleDoor(WorldEntityState entity)
    {
        entity.OpenState = !entity.OpenState;
        worldFacade.NotifyEntityStateChanged(entity);
    }

    private void SetPrimaryBed(WorldEntityState entity)
    {
        if (runtime.PrimaryBedEntityId == entity.Id)
        {
            entity.PlaceableState ??= new PlaceableRuntimeState();
            entity.PlaceableState.AssignedAsPrimaryBed = true;
            worldFacade.NotifyEntityStateChanged(entity);
            return;
        }

        if (runtime.PrimaryBedEntityId is { } previousId
            && worldQuery.TryGetActiveEntity(previousId, out var previousBed))
        {
            previousBed.PlaceableState ??= new PlaceableRuntimeState();
            previousBed.PlaceableState.AssignedAsPrimaryBed = false;
            worldFacade.NotifyEntityStateChanged(previousBed);
        }

        runtime.PrimaryBedEntityId = entity.Id;
        entity.PlaceableState ??= new PlaceableRuntimeState();
        entity.PlaceableState.AssignedAsPrimaryBed = true;
        worldFacade.NotifyEntityStateChanged(entity);
    }

    private void SleepUntilMorning()
    {
        var dayLength = runtime.BootstrapConfig.DayLengthSeconds;
        if (dayLength <= 0f)
        {
            return;
        }

        var remaining = dayLength - runtime.WorldState.TimeOfDaySeconds;
        if (remaining <= 0f)
        {
            remaining = dayLength;
        }

        runtime.WorldState.TotalElapsedSeconds += remaining;
        runtime.WorldState.TimeOfDaySeconds = 0f;
        ClearTransientUiState();
    }

    private void ValidateActiveStorage()
    {
        if (runtime.WorldState.ActiveStorageEntityId is not { } storageId)
        {
            return;
        }

        var entity = worldQuery.TryGetActiveEntity(storageId, out var activeEntity) ? activeEntity : null;
        if (entity is not null && !entity.Removed && Vector2.Distance(entity.Position, runtime.Player.Position) <= StationRange)
        {
            return;
        }

        CloseActiveStorage();
    }

    private void ValidateActiveStation()
    {
        if (runtime.WorldState.ActiveStationEntityId is not { } activeId)
        {
            return;
        }

        var entity = worldQuery.TryGetActiveEntity(activeId, out var activeEntity) ? activeEntity : null;
        if (entity is null || Vector2.Distance(entity.Position, runtime.Player.Position) > StationRange)
        {
            runtime.WorldState.ActiveStationEntityId = null;
            runtime.WorldState.ActiveStationKind = null;
            if (runtime.WorldState.WorkspaceMode is CraftWorkspaceMode.Workbench or CraftWorkspaceMode.Furnace)
            {
                runtime.WorldState.WorkspaceMode = CraftWorkspaceMode.Hidden;
            }
        }
    }

    private void CloseActiveStorage()
    {
        if (runtime.WorldState.ActiveStorageEntityId is { } storageId
            && worldQuery.TryGetActiveEntity(storageId, out var storageEntity)
            && !storageEntity.Removed)
        {
            storageEntity.OpenState = false;
            worldFacade.NotifyEntityStateChanged(storageEntity);
        }

        runtime.WorldState.ActiveStorageEntityId = null;
    }

    private void SyncFuel(WorldEntityState entity)
    {
        var state = entity.PlaceableState;
        if (state is null || !state.IsLit || state.FuelSecondsRemaining <= 0f)
        {
            return;
        }

        var elapsed = runtime.WorldState.TotalElapsedSeconds - state.FuelLastUpdatedTotalSeconds;
        if (elapsed <= 0f)
        {
            return;
        }

        state.FuelSecondsRemaining = Math.Max(0f, state.FuelSecondsRemaining - elapsed);
        state.FuelLastUpdatedTotalSeconds = runtime.WorldState.TotalElapsedSeconds;
        if (state.FuelSecondsRemaining <= 0f)
        {
            state.IsLit = false;
        }
    }
}
