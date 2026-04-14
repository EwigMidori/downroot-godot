using System.Numerics;
using Downroot.Core.Gameplay;
using Downroot.Core.Ids;

namespace Downroot.Gameplay.Runtime;

public sealed class WorldQueryService(GameRuntime runtime, WorldRuntimeFacade worldFacade)
{
    public IReadOnlyList<WorldEntityState> EnumerateActiveEntities() => worldFacade.GetActiveProjection();

    public bool TryGetActiveEntity(EntityId entityId, out WorldEntityState entity)
    {
        return worldFacade.TryGetActiveEntity(entityId, out entity);
    }

    public WorldEntityState? GetNearestInteractable(float range)
    {
        return FindNearest(range, IsInteractionEligible);
    }

    public WorldEntityState? GetNearestCreature(float range)
    {
        return FindNearest(range, static entity => entity.Kind == WorldEntityKind.Creature);
    }

    public WorldEntityState? GetNearestDestructibleEntity(float range)
    {
        return FindNearest(range, IsDestructible);
    }

    public WorldEntityState? FindNearbyStation(CraftingStationKind stationKind, float range)
    {
        return FindNearest(range, entity =>
        {
            return entity.Kind == WorldEntityKind.Placeable
                && runtime.Content.Placeables.Get(entity.DefinitionId).HasBehavior(Downroot.Core.Definitions.PlaceableBehaviorKind.CraftingStation)
                && runtime.Content.Placeables.Get(entity.DefinitionId).CraftingStationKind == stationKind;
        });
    }

    public bool HasAnyEntityNear(Vector2 position, float radius)
    {
        var radiusSquared = radius * radius;
        foreach (var entity in EnumerateActiveEntities())
        {
            if (entity.Removed)
            {
                continue;
            }

            if (Vector2.DistanceSquared(entity.Position, position) < radiusSquared)
            {
                return true;
            }
        }

        return false;
    }

    public IReadOnlyList<WorldEntityState> GetActiveEntities() => EnumerateActiveEntities();

    private WorldEntityState? FindNearest(float range, Func<WorldEntityState, bool> predicate)
    {
        var bestDistanceSquared = range * range;
        WorldEntityState? best = null;
        foreach (var entity in EnumerateActiveEntities())
        {
            if (entity.Removed || !predicate(entity))
            {
                continue;
            }

            var distanceSquared = Vector2.DistanceSquared(entity.Position, runtime.Player.Position);
            if (distanceSquared > bestDistanceSquared)
            {
                continue;
            }

            bestDistanceSquared = distanceSquared;
            best = entity;
        }

        return best;
    }

    private bool IsInteractionEligible(WorldEntityState entity)
    {
        return entity.Kind switch
        {
            WorldEntityKind.ItemDrop => true,
            WorldEntityKind.Placeable => true,
            WorldEntityKind.ResourceNode => IsResourceInteractionEligible(entity),
            _ => false
        };
    }

    private bool IsResourceInteractionEligible(WorldEntityState entity)
    {
        var def = runtime.Content.ResourceNodes.Get(entity.DefinitionId);
        return def.InstantPickup || def.DirectConsume;
    }

    private bool IsDestructible(WorldEntityState entity)
    {
        return entity.Kind switch
        {
            WorldEntityKind.ResourceNode => true,
            WorldEntityKind.Placeable => runtime.Content.Placeables.Get(entity.DefinitionId).CanBeDestroyed,
            _ => false
        };
    }
}
