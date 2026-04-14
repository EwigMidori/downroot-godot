using System.Numerics;
using Downroot.Core.Gameplay;
using Downroot.Core.Ids;
using Downroot.Core.Input;
using Downroot.Core.World;

namespace Downroot.Gameplay.Runtime.Systems;

public sealed class InteractionSystem(
    GameRuntime runtime,
    WorldQueryService worldQuery,
    PortalTravelSystem portalTravelSystem)
{
    private const float InteractionRange = 48f;
    private const float StationRange = 56f;

    public void ValidateActiveStation()
    {
        if (runtime.WorldState.ActiveStationEntityId is not { } activeId)
        {
            return;
        }

        var entity = worldQuery.GetActiveEntities().FirstOrDefault(candidate => !candidate.Removed && candidate.Id == activeId);
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

    public void UpdateInteractionContext()
    {
        runtime.WorldState.CurrentInteraction = worldQuery.GetNearestInteractable(InteractionRange) switch
        {
            null => null,
            { Kind: WorldEntityKind.ResourceNode } entity => CreateResourceInteractionContext(entity),
            { Kind: WorldEntityKind.Placeable } entity => CreatePlaceableInteractionContext(entity),
            { Kind: WorldEntityKind.ItemDrop } entity => new InteractionContext(entity.Id, entity.Kind, entity.DefinitionId, InteractionVerb.PickUp),
            _ => null
        };
    }

    public void HandleInteract(InputFrame input)
    {
        if (!input.InteractPressed)
        {
            return;
        }

        var target = worldQuery.GetNearestInteractable(InteractionRange);
        if (target is null)
        {
            return;
        }

        switch (target.Kind)
        {
            case WorldEntityKind.ResourceNode:
                InteractResourceNode(target);
                break;
            case WorldEntityKind.Placeable:
                InteractPlaceable(target);
                break;
            case WorldEntityKind.ItemDrop:
                PickupDrop(target);
                break;
        }
    }

    public bool TryGetNearbyStation(CraftingStationKind stationKind, out WorldEntityState station)
    {
        station = worldQuery.FindNearbyStation(stationKind, StationRange)!;
        return station is not null;
    }

    private void InteractResourceNode(WorldEntityState entity)
    {
        var def = runtime.Content.ResourceNodes.Get(entity.DefinitionId);
        if (def.DirectConsume)
        {
            runtime.Player.Survival.RestoreHunger(def.HungerRestore);
            entity.Removed = true;
            return;
        }

        if (def.InstantPickup)
        {
            foreach (var drop in def.Drops)
            {
                runtime.Player.Inventory.TryAdd(drop.ItemId, drop.Amount, runtime.Content);
            }

            entity.Removed = true;
        }
    }

    private void InteractPlaceable(WorldEntityState entity)
    {
        if (IsPortalEntity(entity))
        {
            portalTravelSystem.StartPortalTravel(entity);
            return;
        }

        var def = runtime.Content.Placeables.Get(entity.DefinitionId);
        if (def.IsCraftingStation && def.CraftingStationKind is not null)
        {
            runtime.WorldState.ActiveStationKind = def.CraftingStationKind;
            runtime.WorldState.ActiveStationEntityId = entity.Id;
            runtime.WorldState.WorkspaceMode = def.CraftingStationKind == CraftingStationKind.Furnace
                ? CraftWorkspaceMode.Furnace
                : CraftWorkspaceMode.Workbench;
        }
        else
        {
            entity.OpenState = !entity.OpenState;
        }
    }

    private void PickupDrop(WorldEntityState entity)
    {
        if (runtime.Player.Inventory.TryAdd(entity.DefinitionId, entity.StackCount, runtime.Content))
        {
            entity.Removed = true;
        }
    }

    private InteractionContext? CreateResourceInteractionContext(WorldEntityState entity)
    {
        var def = runtime.Content.ResourceNodes.Get(entity.DefinitionId);
        if (def.DirectConsume)
        {
            return new InteractionContext(entity.Id, entity.Kind, entity.DefinitionId, InteractionVerb.Eat);
        }

        if (def.InstantPickup)
        {
            return new InteractionContext(entity.Id, entity.Kind, entity.DefinitionId, InteractionVerb.Gather);
        }

        return null;
    }

    private InteractionContext CreatePlaceableInteractionContext(WorldEntityState entity)
    {
        if (IsPortalEntity(entity))
        {
            return new InteractionContext(entity.Id, entity.Kind, entity.DefinitionId, InteractionVerb.Use);
        }

        var def = runtime.Content.Placeables.Get(entity.DefinitionId);
        if (def.IsCraftingStation)
        {
            return new InteractionContext(entity.Id, entity.Kind, entity.DefinitionId, InteractionVerb.Use);
        }

        if (def.HasOpenVariant)
        {
            return new InteractionContext(entity.Id, entity.Kind, entity.DefinitionId, entity.OpenState ? InteractionVerb.Close : InteractionVerb.Open);
        }

        return new InteractionContext(entity.Id, entity.Kind, entity.DefinitionId, InteractionVerb.Use);
    }

    private bool IsPortalEntity(WorldEntityState entity)
    {
        if (entity.Kind != WorldEntityKind.Placeable || !entity.IsNatural)
        {
            return false;
        }

        var portalDefId = GetPortalDefinitionId(entity.WorldSpaceKind);
        return portalDefId is not null
            && entity.DefinitionId == portalDefId.Value
            && runtime.Content.PortalWorldLinks.Any(link =>
                (link.SourceWorldSpaceKind == entity.WorldSpaceKind && link.SourcePortalChunk == entity.ChunkCoord)
                || (link.TargetWorldSpaceKind == entity.WorldSpaceKind && link.TargetPortalChunk == entity.ChunkCoord));
    }

    private ContentId? GetPortalDefinitionId(WorldSpaceKind worldSpaceKind)
    {
        return runtime.Content.WorldGenPasses
            .FirstOrDefault(pass => pass.WorldSpaceKind == worldSpaceKind && pass.PassType == WorldGenPassTypes.PortalSite)
            ?.TargetId;
    }
}
