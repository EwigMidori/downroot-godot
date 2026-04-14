using System.Numerics;
using Downroot.Core.Input;

namespace Downroot.Gameplay.Runtime.Systems;

public sealed class PlacementSystem(GameRuntime runtime, WorldRuntimeFacade worldFacade, WorldQueryService worldQuery, MovementSystem movementSystem)
{
    public void HandlePlacement(InputFrame input)
    {
        if (!input.PlacePressed)
        {
            return;
        }

        var slot = runtime.Player.Inventory.Slots[runtime.Player.SelectedHotbarIndex];
        if (slot.ItemId is null || !runtime.Content.Items.TryGet(slot.ItemId.Value, out var itemDef) || itemDef!.PlaceableId is null)
        {
            return;
        }

        var tileCoord = worldFacade.GetWorldTile(input.PointerWorld);
        var tile = worldFacade.GetWorldPosition(tileCoord);
        if (worldQuery.GetActiveEntities().Any(entity => !entity.Removed && Vector2.Distance(entity.Position, tile) < 8f))
        {
            return;
        }

        if (movementSystem.IsBlocked(tile))
        {
            return;
        }

        var placeableDef = runtime.Content.Placeables.Get(itemDef.PlaceableId.Value);
        worldFacade.GetActiveWorld().AddRuntimeEntity(new WorldEntityState(
            WorldEntityKind.Placeable,
            placeableDef.Id,
            tile,
            placeableDef.MaxDurability,
            runtime.ActiveWorldSpaceKind,
            tileCoord.ToChunkCoord(runtime.ChunkWidth, runtime.ChunkHeight)));
        worldFacade.RefreshEntityProjection();
        slot.Remove(1);
    }
}
