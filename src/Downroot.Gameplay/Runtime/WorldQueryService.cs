using System.Numerics;
using Downroot.Core.Gameplay;

namespace Downroot.Gameplay.Runtime;

public sealed class WorldQueryService(GameRuntime runtime, WorldRuntimeFacade worldFacade)
{
    public WorldEntityState? GetNearestInteractable(float range)
    {
        return EnumerateActiveEntities()
            .Where(entity => !entity.Removed && IsInteractionEligible(entity))
            .OrderBy(entity => Vector2.Distance(entity.Position, runtime.Player.Position))
            .FirstOrDefault(entity => Vector2.Distance(entity.Position, runtime.Player.Position) <= range);
    }

    public WorldEntityState? GetNearestCreature(float range)
    {
        return EnumerateActiveEntities()
            .Where(entity => !entity.Removed && entity.Kind == WorldEntityKind.Creature)
            .OrderBy(entity => Vector2.Distance(entity.Position, runtime.Player.Position))
            .FirstOrDefault(entity => Vector2.Distance(entity.Position, runtime.Player.Position) <= range);
    }

    public WorldEntityState? GetNearestDestructibleEntity(float range)
    {
        return EnumerateActiveEntities()
            .Where(entity => !entity.Removed && IsDestructible(entity))
            .OrderBy(entity => Vector2.Distance(entity.Position, runtime.Player.Position))
            .FirstOrDefault(entity => Vector2.Distance(entity.Position, runtime.Player.Position) <= range);
    }

    public WorldEntityState? FindNearbyStation(CraftingStationKind stationKind, float range)
    {
        return EnumerateActiveEntities()
            .Where(entity => !entity.Removed && entity.Kind == WorldEntityKind.Placeable)
            .OrderBy(entity => Vector2.Distance(entity.Position, runtime.Player.Position))
            .FirstOrDefault(entity =>
            {
                if (Vector2.Distance(entity.Position, runtime.Player.Position) > range)
                {
                    return false;
                }

                return runtime.Content.Placeables.Get(entity.DefinitionId).CraftingStationKind == stationKind;
            });
    }

    public IReadOnlyList<WorldEntityState> GetActiveEntities()
    {
        return EnumerateActiveEntities()
            .Where(entity => !entity.Removed)
            .OrderBy(entity => entity.Position.Y)
            .ThenBy(entity => entity.Position.X)
            .ToArray();
    }

    private IEnumerable<WorldEntityState> EnumerateActiveEntities() => worldFacade.GetActiveWorld().EnumerateLoadedEntities();

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
