using System.Numerics;
using Downroot.Core.Gameplay;
using Downroot.Core.Ids;
using Downroot.Core.Input;
using Downroot.Core.World;

namespace Downroot.Gameplay.Runtime.Systems;

public sealed class InteractionSystem(
    GameRuntime runtime,
    WorldRuntimeFacade worldFacade,
    WorldQueryService worldQuery,
    PortalTravelSystem portalTravelSystem)
{
    private const float InteractionRange = 48f;
    private readonly PlaceableInteractionResolver _placeableResolver = new(runtime, worldFacade, worldQuery);

    public void ValidateActiveStation()
    {
        _placeableResolver.ValidateActiveInteractions();
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
        return _placeableResolver.TryGetNearbyStation(stationKind, out station);
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
        if (worldFacade.IsPortalEntity(entity))
        {
            portalTravelSystem.StartPortalTravel(entity);
            return;
        }

        var def = runtime.Content.Placeables.Get(entity.DefinitionId);
        _placeableResolver.Interact(entity);
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
        if (worldFacade.IsPortalEntity(entity))
        {
            return new InteractionContext(entity.Id, entity.Kind, entity.DefinitionId, InteractionVerb.Use);
        }

        return _placeableResolver.CreateInteractionContext(entity);
    }
}
