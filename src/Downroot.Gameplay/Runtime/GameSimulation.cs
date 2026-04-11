using System.Numerics;
using Downroot.Content.Registries;
using Downroot.Core.Definitions;
using Downroot.Core.Ids;
using Downroot.Core.Input;

namespace Downroot.Gameplay.Runtime;

public sealed class GameSimulation(GameRuntime runtime)
{
    private const float InteractionRange = 48f;

    public void Tick(float deltaSeconds, InputFrame input)
    {
        UpdatePlayerMovement(deltaSeconds, input.Movement);
        UpdateHotbarSelection(input);
        UpdateWorldTime(deltaSeconds);
        UpdateInteractionPrompt();
        HandleToggles(input);
        HandleInteract(input);
        HandleConsumption(input);
        HandlePlacement(input);
        HandleDestroy(deltaSeconds, input);
        UpdateCreatures(deltaSeconds);
        runtime.WorldState.RemoveDeleted();
    }

    public IReadOnlyList<RecipeDef> GetAvailableRecipes()
    {
        return runtime.Content.Recipes.All
            .Where(recipe => recipe.RequiredStationKey is null || recipe.RequiredStationKey == runtime.WorldState.ActiveStationKey)
            .ToArray();
    }

    public bool Craft(ContentId recipeId)
    {
        var recipe = runtime.Content.Recipes.Get(recipeId);
        if (recipe.RequiredStationKey is not null && recipe.RequiredStationKey != runtime.WorldState.ActiveStationKey)
        {
            return false;
        }

        if (recipe.Ingredients.Any(ingredient => !runtime.Player.Inventory.Has(ingredient.ItemId, ingredient.Amount)))
        {
            return false;
        }

        foreach (var ingredient in recipe.Ingredients)
        {
            runtime.Player.Inventory.TryConsume(ingredient.ItemId, ingredient.Amount);
        }

        return runtime.Player.Inventory.TryAdd(recipe.Result.ItemId, recipe.Result.Amount, runtime.Content);
    }

    private void UpdatePlayerMovement(float deltaSeconds, Vector2 movement)
    {
        if (movement != Vector2.Zero)
        {
            var normalized = Vector2.Normalize(movement);
            runtime.Player.Facing = normalized;
            runtime.Player.Position += normalized * runtime.Player.Speed * deltaSeconds;
        }
    }

    private void UpdateHotbarSelection(InputFrame input)
    {
        if (input.DirectHotbarSlot is { } directIndex)
        {
            runtime.Player.SelectedHotbarIndex = Math.Clamp(directIndex, 0, runtime.Player.HotbarSize - 1);
        }

        if (input.HotbarScrollDelta != 0)
        {
            var next = runtime.Player.SelectedHotbarIndex + input.HotbarScrollDelta;
            runtime.Player.SelectedHotbarIndex = ((next % runtime.Player.HotbarSize) + runtime.Player.HotbarSize) % runtime.Player.HotbarSize;
        }
    }

    private void UpdateWorldTime(float deltaSeconds)
    {
        runtime.WorldState.TotalElapsedSeconds += deltaSeconds;
        runtime.WorldState.TimeOfDaySeconds += deltaSeconds;

        if (runtime.WorldState.TimeOfDaySeconds >= runtime.BootstrapConfig.DayLengthSeconds)
        {
            runtime.WorldState.TimeOfDaySeconds -= runtime.BootstrapConfig.DayLengthSeconds;
        }

        if (runtime.WorldState.TotalElapsedSeconds % 3f < deltaSeconds)
        {
            runtime.Player.Survival.DrainHunger(1);
            if (runtime.Player.Survival.Hunger == 0)
            {
                runtime.Player.Survival.Damage(1);
            }
        }
    }

    private void UpdateInteractionPrompt()
    {
        var target = GetNearestInteractable();
        runtime.WorldState.InteractionPrompt = target is null ? string.Empty : "[F] Interact";
    }

    private void HandleToggles(InputFrame input)
    {
        if (input.InventoryToggled)
        {
            runtime.WorldState.InventoryVisible = !runtime.WorldState.InventoryVisible;
        }

        if (input.CraftPressed)
        {
            runtime.WorldState.CraftingVisible = !runtime.WorldState.CraftingVisible;
            if (!runtime.WorldState.CraftingVisible)
            {
                runtime.WorldState.ActiveStationKey = null;
            }
        }
    }

    private void HandleInteract(InputFrame input)
    {
        if (!input.InteractPressed)
        {
            return;
        }

        var target = GetNearestInteractable();
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

    private void HandleConsumption(InputFrame input)
    {
        if (!input.ConsumePressed)
        {
            return;
        }

        var slot = runtime.Player.Inventory.Slots[runtime.Player.SelectedHotbarIndex];
        if (slot.ItemId is null || !runtime.Content.Items.TryGet(slot.ItemId.Value, out var itemDef))
        {
            return;
        }

        if (itemDef!.HungerRestore <= 0 && itemDef.HealthRestore <= 0)
        {
            return;
        }

        slot.Remove(1);
        if (itemDef.HungerRestore > 0)
        {
            runtime.Player.Survival.RestoreHunger(itemDef.HungerRestore);
        }

        if (itemDef.HealthRestore > 0)
        {
            runtime.Player.Survival.Heal(itemDef.HealthRestore);
        }
    }

    private void HandlePlacement(InputFrame input)
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

        var tile = new Vector2(MathF.Floor(input.PointerWorld.X / 32f) * 32f, MathF.Floor(input.PointerWorld.Y / 32f) * 32f);
        if (runtime.WorldState.Entities.Any(entity => !entity.Removed && Vector2.Distance(entity.Position, tile) < 8f))
        {
            return;
        }

        var placeableDef = runtime.Content.Placeables.Get(itemDef.PlaceableId.Value);
        runtime.WorldState.AddEntity(new WorldEntityState(
            WorldEntityKind.Placeable,
            placeableDef.Id,
            tile,
            placeableDef.MaxDurability));
        slot.Remove(1);
    }

    private void HandleDestroy(float deltaSeconds, InputFrame input)
    {
        var target = GetNearestDestructible();
        if (target is null || !input.DestroyHeld)
        {
            runtime.WorldState.DestroyProgress01 = 0;
            return;
        }

        target.DamageAccumulator += deltaSeconds;
        runtime.WorldState.DestroyProgress01 = Math.Clamp(target.DamageAccumulator / Math.Max(1, target.Durability), 0f, 1f);
        if (target.DamageAccumulator < Math.Max(1, target.Durability))
        {
            return;
        }

        target.DamageAccumulator = 0f;
        target.Durability -= 1;
        if (target.Durability > 0)
        {
            runtime.WorldState.DestroyProgress01 = 0f;
            return;
        }

        DestroyEntity(target);
        runtime.WorldState.DestroyProgress01 = 0f;
    }

    private void UpdateCreatures(float deltaSeconds)
    {
        var isNight = runtime.WorldState.IsNight(runtime.BootstrapConfig.DayLengthSeconds);

        foreach (var creature in runtime.WorldState.Entities.Where(entity => entity.Kind == WorldEntityKind.Creature && !entity.Removed))
        {
            var def = runtime.Content.Creatures.Get(creature.DefinitionId);
            if (!def.NightOnlyAggro || !isNight)
            {
                continue;
            }

            var direction = runtime.Player.Position - creature.Position;
            if (direction != Vector2.Zero)
            {
                creature.Position += Vector2.Normalize(direction) * def.MoveSpeed * deltaSeconds;
            }

            creature.AiAccumulator += deltaSeconds;
            if (Vector2.Distance(creature.Position, runtime.Player.Position) < 18f && creature.AiAccumulator >= 1f)
            {
                runtime.Player.Survival.Damage(def.ContactDamage);
                creature.AiAccumulator = 0f;
            }
        }
    }

    private WorldEntityState? GetNearestInteractable()
    {
        return runtime.WorldState.Entities
            .Where(entity => !entity.Removed && entity.Kind is WorldEntityKind.ResourceNode or WorldEntityKind.Placeable or WorldEntityKind.ItemDrop)
            .OrderBy(entity => Vector2.Distance(entity.Position, runtime.Player.Position))
            .FirstOrDefault(entity => Vector2.Distance(entity.Position, runtime.Player.Position) <= InteractionRange);
    }

    private WorldEntityState? GetNearestDestructible()
    {
        return runtime.WorldState.Entities
            .Where(entity => !entity.Removed && entity.Kind is WorldEntityKind.ResourceNode or WorldEntityKind.Placeable)
            .OrderBy(entity => Vector2.Distance(entity.Position, runtime.Player.Position))
            .FirstOrDefault(entity => Vector2.Distance(entity.Position, runtime.Player.Position) <= InteractionRange);
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
        var def = runtime.Content.Placeables.Get(entity.DefinitionId);
        if (def.IsCraftingStation && def.CraftingStationKey is not null)
        {
            runtime.WorldState.ActiveStationKey = def.CraftingStationKey;
            runtime.WorldState.CraftingVisible = true;
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

    private void DestroyEntity(WorldEntityState entity)
    {
        entity.Removed = true;

        switch (entity.Kind)
        {
            case WorldEntityKind.ResourceNode:
                var resourceDef = runtime.Content.ResourceNodes.Get(entity.DefinitionId);
                foreach (var drop in resourceDef.Drops)
                {
                    runtime.WorldState.AddEntity(new WorldEntityState(WorldEntityKind.ItemDrop, drop.ItemId, entity.Position, 1, drop.Amount));
                }
                break;
            case WorldEntityKind.Placeable:
                var itemDef = runtime.Content.Items.All.FirstOrDefault(item => item.PlaceableId == entity.DefinitionId);
                if (itemDef is not null)
                {
                    runtime.WorldState.AddEntity(new WorldEntityState(WorldEntityKind.ItemDrop, itemDef.Id, entity.Position, 1, 1));
                }
                break;
        }
    }
}
