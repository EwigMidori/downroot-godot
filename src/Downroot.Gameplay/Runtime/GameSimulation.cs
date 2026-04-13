using System.Numerics;
using Downroot.Content.Registries;
using Downroot.Core.Definitions;
using Downroot.Core.Ids;
using Downroot.Core.Input;

namespace Downroot.Gameplay.Runtime;

public sealed class GameSimulation(GameRuntime runtime)
{
    private const float InteractionRange = 48f;
    private const float StationRange = 56f;
    private const float BlockingRadius = 18f;
    private const float AttackRange = 28f;
    private const int EmptyHandDamage = 1;
    private bool _previousDestroyHeld;
    private bool _suppressDestroyUntilRelease;

    public void Tick(float deltaSeconds, InputFrame input)
    {
        if (!input.DestroyHeld)
        {
            _suppressDestroyUntilRelease = false;
        }

        runtime.WorldState.TickStatusEvent(deltaSeconds);
        ValidateActiveStation();
        UpdatePlayerMovement(deltaSeconds, input.Movement);
        UpdateHotbarSelection(input);
        UpdateWorldTime(deltaSeconds);
        UpdateFurnaceTask(deltaSeconds);
        UpdateInteractionContext();
        HandleToggles(input);
        HandleInteract(input);
        HandleAttack(input);
        HandleConsumption(input);
        HandlePlacement(input);
        HandleDestroy(deltaSeconds, input);
        UpdateCreatures(deltaSeconds);
        runtime.WorldState.RemoveDeleted();
        _previousDestroyHeld = input.DestroyHeld;
    }

    public IReadOnlyList<RecipeDef> GetRecipesForWorkspace(CraftWorkspaceMode workspaceMode)
    {
        return workspaceMode switch
        {
            CraftWorkspaceMode.Handcraft => runtime.Content.Recipes.All.Where(recipe => recipe.RequiredStationKey is null).ToArray(),
            CraftWorkspaceMode.Workbench => runtime.Content.Recipes.All.Where(recipe => recipe.RequiredStationKey == "workbench").ToArray(),
            CraftWorkspaceMode.Furnace => runtime.Content.Recipes.All.Where(recipe => recipe.RequiredStationKey == "furnace").ToArray(),
            _ => []
        };
    }

    public bool Craft(ContentId recipeId)
    {
        return TryCraft(recipeId, out _);
    }

    public bool TryCraft(ContentId recipeId, out string failureReason)
    {
        var recipe = runtime.Content.Recipes.Get(recipeId);
        var outputs = GetRecipeOutputs(recipe);
        if (recipe.RequiredStationKey is not null && !IsStationAvailable(recipe.RequiredStationKey))
        {
            failureReason = $"{recipe.DisplayName} requires a nearby workbench.";
            PublishCraftResult(recipe, false, failureReason, new StatusEventState(StatusEventKind.StationRequired, recipe.Result.ItemId));
            return false;
        }

        var missingIngredient = recipe.Ingredients.FirstOrDefault(ingredient => !runtime.Player.Inventory.Has(ingredient.ItemId, ingredient.Amount));
        if (missingIngredient is not null)
        {
            failureReason = $"Missing {ShortName(missingIngredient.ItemId)} x{missingIngredient.Amount}.";
            PublishCraftResult(recipe, false, failureReason, new StatusEventState(StatusEventKind.MissingIngredient, missingIngredient.ItemId, missingIngredient.Amount));
            return false;
        }

        if (!runtime.Player.Inventory.CanAddMany(outputs, runtime.Content))
        {
            failureReason = $"No inventory space for {recipe.DisplayName}.";
            PublishCraftResult(recipe, false, failureReason, new StatusEventState(StatusEventKind.InventoryFull, recipe.Result.ItemId));
            return false;
        }

        if (recipe.CraftDurationSeconds > 0f)
        {
            if (runtime.WorldState.ActiveFurnaceTask is not null)
            {
                failureReason = "Furnace is already processing another recipe.";
                PublishCraftResult(recipe, false, failureReason, new StatusEventState(StatusEventKind.CraftFailed, recipe.Result.ItemId));
                return false;
            }

            if (runtime.WorldState.ActiveStationEntityId is not { } furnaceEntityId || runtime.WorldState.WorkspaceMode != CraftWorkspaceMode.Furnace)
            {
                failureReason = "Need an active furnace.";
                PublishCraftResult(recipe, false, failureReason, new StatusEventState(StatusEventKind.StationRequired, recipe.Result.ItemId));
                return false;
            }

            runtime.WorldState.ActiveFurnaceTask = new FurnaceTaskState(recipe.Id, furnaceEntityId, recipe.CraftDurationSeconds);
            failureReason = string.Empty;
            runtime.WorldState.SetStatusEvent(new StatusEventState(StatusEventKind.SmeltingStarted, recipe.Result.ItemId), 1.5f);
            Console.WriteLine($"[Smelt] Started {recipe.Id.Value} at furnace {furnaceEntityId.Value}");
            return true;
        }

        foreach (var ingredient in recipe.Ingredients)
        {
            runtime.Player.Inventory.TryConsume(ingredient.ItemId, ingredient.Amount);
        }

        foreach (var output in outputs)
        {
            if (!runtime.Player.Inventory.TryAdd(output.ItemId, output.Amount, runtime.Content))
            {
                failureReason = $"Failed to add {recipe.DisplayName} to inventory.";
                PublishCraftResult(recipe, false, failureReason, new StatusEventState(StatusEventKind.CraftFailed, recipe.Result.ItemId));
                return false;
            }
        }

        failureReason = string.Empty;
        PublishCraftResult(recipe, true, $"Crafted {recipe.DisplayName}.", new StatusEventState(StatusEventKind.CraftedItem, recipe.Result.ItemId, recipe.Result.Amount));
        return true;
    }

    private void UpdatePlayerMovement(float deltaSeconds, Vector2 movement)
    {
        if (movement != Vector2.Zero)
        {
            var normalized = Vector2.Normalize(movement);
            runtime.Player.Facing = normalized;
            runtime.Player.Position = MoveWithCollision(runtime.Player.Position, normalized * runtime.Player.Speed * deltaSeconds);
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
                DamagePlayer(1);
            }
        }
    }

    private void UpdateFurnaceTask(float deltaSeconds)
    {
        var task = runtime.WorldState.ActiveFurnaceTask;
        if (task is null)
        {
            return;
        }

        var furnace = runtime.WorldState.Entities.FirstOrDefault(entity => !entity.Removed && entity.Id == task.FurnaceEntityId);
        if (furnace is null)
        {
            runtime.WorldState.ActiveFurnaceTask = null;
            return;
        }

        task.ElapsedSeconds += deltaSeconds;
        if (task.ElapsedSeconds < task.DurationSeconds)
        {
            return;
        }

        var recipe = runtime.Content.Recipes.Get(task.RecipeId);
        var outputs = GetRecipeOutputs(recipe);
        var missingIngredient = recipe.Ingredients.FirstOrDefault(ingredient => !runtime.Player.Inventory.Has(ingredient.ItemId, ingredient.Amount));
        if (missingIngredient is not null)
        {
            runtime.WorldState.ActiveFurnaceTask = null;
            runtime.WorldState.SetStatusEvent(new StatusEventState(StatusEventKind.MissingIngredient, missingIngredient.ItemId, missingIngredient.Amount));
            Console.WriteLine($"[Smelt][Blocked] {recipe.Id.Value}: missing {missingIngredient.ItemId.Value} x{missingIngredient.Amount}");
            return;
        }

        if (!runtime.Player.Inventory.CanAddMany(outputs, runtime.Content))
        {
            runtime.WorldState.ActiveFurnaceTask = null;
            runtime.WorldState.SetStatusEvent(new StatusEventState(StatusEventKind.InventoryFull, recipe.Result.ItemId));
            Console.WriteLine($"[Smelt][Blocked] {recipe.Id.Value}: inventory full");
            return;
        }

        foreach (var ingredient in recipe.Ingredients)
        {
            runtime.Player.Inventory.TryConsume(ingredient.ItemId, ingredient.Amount);
        }

        foreach (var output in outputs)
        {
            runtime.Player.Inventory.TryAdd(output.ItemId, output.Amount, runtime.Content);
        }

        runtime.WorldState.ActiveFurnaceTask = null;
        runtime.WorldState.SetStatusEvent(new StatusEventState(StatusEventKind.SmeltingCompleted, recipe.Result.ItemId, recipe.Result.Amount));
        Console.WriteLine($"[Smelt] Completed {recipe.Id.Value}");
    }

    private void UpdateInteractionContext()
    {
        runtime.WorldState.CurrentInteraction = GetNearestInteractable() switch
        {
            null => null,
            { Kind: WorldEntityKind.ResourceNode } entity => CreateResourceInteractionContext(entity),
            { Kind: WorldEntityKind.Placeable } entity => CreatePlaceableInteractionContext(entity),
            { Kind: WorldEntityKind.ItemDrop } entity => new InteractionContext(entity.Id, entity.Kind, entity.DefinitionId, InteractionVerb.PickUp),
            _ => null
        };
    }

    private void HandleToggles(InputFrame input)
    {
        if (input.CraftPressed)
        {
            if (runtime.WorldState.WorkspaceMode != CraftWorkspaceMode.Hidden)
            {
                runtime.WorldState.WorkspaceMode = CraftWorkspaceMode.Hidden;
                runtime.WorldState.ActiveStationKey = null;
                runtime.WorldState.ActiveStationEntityId = null;
                return;
            }

            if (TryGetNearbyWorkbench(out var station))
            {
                var stationDef = runtime.Content.Placeables.Get(station.DefinitionId);
                runtime.WorldState.ActiveStationKey = stationDef.CraftingStationKey;
                runtime.WorldState.ActiveStationEntityId = station.Id;
                runtime.WorldState.WorkspaceMode = CraftWorkspaceMode.Workbench;
                return;
            }

            runtime.WorldState.WorkspaceMode = CraftWorkspaceMode.Handcraft;
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

    private void HandleAttack(InputFrame input)
    {
        var attackPressed = input.DestroyHeld && !_previousDestroyHeld;
        if (!attackPressed)
        {
            return;
        }

        var target = GetNearestCreature(AttackRange);
        if (target is null)
        {
            return;
        }

        var selectedItem = GetSelectedItemDef();
        var damage = selectedItem?.MeleeDamage is > 0
            ? selectedItem.MeleeDamage
            : EmptyHandDamage;
        DamageCreature(target, damage);
        _suppressDestroyUntilRelease = true;
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

        if (IsBlocked(tile))
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
        if (_suppressDestroyUntilRelease)
        {
            runtime.WorldState.ActiveDestroyProgress = null;
            return;
        }

        if (GetSelectedItemDef()?.MeleeDamage is > 0 && GetNearestCreature(AttackRange) is not null)
        {
            runtime.WorldState.ActiveDestroyProgress = null;
            return;
        }

        var target = GetNearestDestructible();
        if (target is null || !input.DestroyHeld)
        {
            runtime.WorldState.ActiveDestroyProgress = null;
            return;
        }

        var breakDuration = GetBreakDuration(target);
        target.DamageAccumulator += deltaSeconds;
        var progress = Math.Clamp(target.DamageAccumulator / breakDuration, 0f, 1f);
        runtime.WorldState.ActiveDestroyProgress = new DestroyProgressState(
            target.Id,
            target.Kind,
            target.DefinitionId,
            target.Position,
            progress);
        if (target.DamageAccumulator < breakDuration)
        {
            return;
        }

        DestroyEntity(target);
        runtime.WorldState.ActiveDestroyProgress = null;
    }

    private void UpdateCreatures(float deltaSeconds)
    {
        var isNight = runtime.WorldState.IsNight(runtime.BootstrapConfig.DayLengthSeconds);

        foreach (var creature in runtime.WorldState.Entities.Where(entity => entity.Kind == WorldEntityKind.Creature && !entity.Removed))
        {
            var def = runtime.Content.Creatures.Get(creature.DefinitionId);
            var distance = Vector2.Distance(creature.Position, runtime.Player.Position);

            if (!isNight && def.DayFleeStartRange > 0f)
            {
                if (distance <= def.DayFleeStartRange)
                {
                    creature.OpenState = true;
                }
                else if (distance >= def.DayFleeStopRange)
                {
                    creature.OpenState = false;
                }

                if (creature.OpenState && distance > 0f)
                {
                    var fleeDirection = Vector2.Normalize(creature.Position - runtime.Player.Position);
                    creature.Position = MoveWithCollision(creature.Position, fleeDirection * def.MoveSpeed * deltaSeconds, creature.Id);
                }
                continue;
            }

            var chase = (def.NightOnlyAggro && isNight) || (def.NightAggroRange > 0f && isNight && distance <= def.NightAggroRange);
            if (!chase)
            {
                continue;
            }

            var direction = runtime.Player.Position - creature.Position;
            if (direction != Vector2.Zero)
            {
                creature.Position = MoveWithCollision(creature.Position, Vector2.Normalize(direction) * def.MoveSpeed * deltaSeconds, creature.Id);
            }

            creature.AiAccumulator += deltaSeconds;
            if (Vector2.Distance(creature.Position, runtime.Player.Position) < 18f && creature.AiAccumulator >= def.ContactDamageCooldownSeconds)
            {
                DamagePlayer(def.ContactDamage);
                creature.AiAccumulator = 0f;
            }
        }
    }

    private WorldEntityState? GetNearestInteractable()
    {
        return runtime.WorldState.Entities
            .Where(entity => !entity.Removed && IsInteractionEligible(entity))
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
            runtime.WorldState.ActiveStationEntityId = entity.Id;
            runtime.WorldState.WorkspaceMode = def.CraftingStationKey == "furnace"
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

    private void DestroyEntity(WorldEntityState entity)
    {
        if (runtime.WorldState.ActiveStationEntityId == entity.Id)
        {
            runtime.WorldState.ActiveStationEntityId = null;
            runtime.WorldState.ActiveStationKey = null;
            runtime.WorldState.WorkspaceMode = CraftWorkspaceMode.Hidden;
        }

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

    private bool IsStationAvailable(string? stationKey)
    {
        if (stationKey is null)
        {
            return true;
        }

        ValidateActiveStation();
        return runtime.WorldState.ActiveStationKey == stationKey && runtime.WorldState.ActiveStationEntityId is not null;
    }

    private void ValidateActiveStation()
    {
        if (runtime.WorldState.ActiveStationEntityId is not { } activeId)
        {
            return;
        }

        var entity = runtime.WorldState.Entities.FirstOrDefault(candidate => !candidate.Removed && candidate.Id == activeId);
        if (entity is null || Vector2.Distance(entity.Position, runtime.Player.Position) > StationRange)
        {
            runtime.WorldState.ActiveStationEntityId = null;
            runtime.WorldState.ActiveStationKey = null;
            if (runtime.WorldState.WorkspaceMode is CraftWorkspaceMode.Workbench or CraftWorkspaceMode.Furnace)
            {
                runtime.WorldState.WorkspaceMode = CraftWorkspaceMode.Hidden;
            }
        }
    }

    private Vector2 MoveWithCollision(Vector2 currentPosition, Vector2 delta, Downroot.Core.Ids.EntityId? ignoreEntityId = null)
    {
        var desired = currentPosition + delta;
        var slideX = new Vector2(desired.X, currentPosition.Y);
        var slideY = new Vector2(currentPosition.X, desired.Y);

        if (!IsBlocked(desired, ignoreEntityId))
        {
            return desired;
        }

        if (!IsBlocked(slideX, ignoreEntityId))
        {
            return slideX;
        }

        if (!IsBlocked(slideY, ignoreEntityId))
        {
            return slideY;
        }

        return currentPosition;
    }

    private bool IsBlocked(Vector2 position, Downroot.Core.Ids.EntityId? ignoreEntityId = null)
    {
        return runtime.WorldState.Entities
            .Where(entity => !entity.Removed && entity.Kind == WorldEntityKind.Placeable && entity.Id != ignoreEntityId)
            .Any(entity =>
            {
                var def = runtime.Content.Placeables.Get(entity.DefinitionId);
                var blocks = entity.OpenState ? def.BlocksMovementWhenOpen : def.BlocksMovement;
                return blocks && Vector2.Distance(entity.Position, position) < BlockingRadius;
            });
    }

    private void PublishCraftResult(RecipeDef recipe, bool success, string message, StatusEventState statusEvent)
    {
        var prefix = success ? "[Craft]" : "[Craft][Blocked]";
        Console.WriteLine($"{prefix} {recipe.Id.Value}: {message}");
        runtime.WorldState.SetStatusEvent(statusEvent);
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

    private bool TryGetNearbyWorkbench(out WorldEntityState station)
    {
        station = runtime.WorldState.Entities
            .Where(entity => !entity.Removed && entity.Kind == WorldEntityKind.Placeable)
            .OrderBy(entity => Vector2.Distance(entity.Position, runtime.Player.Position))
            .FirstOrDefault(entity =>
            {
                if (Vector2.Distance(entity.Position, runtime.Player.Position) > StationRange)
                {
                    return false;
                }

                return runtime.Content.Placeables.Get(entity.DefinitionId).CraftingStationKey == "workbench";
            })!;

        return station is not null;
    }

    private IReadOnlyList<ItemAmount> GetRecipeOutputs(RecipeDef recipe)
    {
        return recipe.ExtraResults is null
            ? [recipe.Result]
            : (new[] { recipe.Result }).Concat(recipe.ExtraResults).ToArray();
    }

    private ItemDef? GetSelectedItemDef()
    {
        var slot = runtime.Player.Inventory.Slots[runtime.Player.SelectedHotbarIndex];
        if (slot.ItemId is null || !runtime.Content.Items.TryGet(slot.ItemId.Value, out var item))
        {
            return null;
        }

        return item;
    }

    private float GetBreakDuration(WorldEntityState target)
    {
        var duration = Math.Max(1, target.Durability);
        if (target.Kind != WorldEntityKind.ResourceNode)
        {
            return duration;
        }

        var resourceDef = runtime.Content.ResourceNodes.Get(target.DefinitionId);
        if (!resourceDef.IsTree)
        {
            return duration;
        }

        var multiplier = GetSelectedItemDef()?.TreeBreakSpeedMultiplier ?? 1f;
        return Math.Max(0.2f, duration / Math.Max(1f, multiplier));
    }

    private WorldEntityState? GetNearestCreature(float range)
    {
        return runtime.WorldState.Entities
            .Where(entity => !entity.Removed && entity.Kind == WorldEntityKind.Creature)
            .OrderBy(entity => Vector2.Distance(entity.Position, runtime.Player.Position))
            .FirstOrDefault(entity => Vector2.Distance(entity.Position, runtime.Player.Position) <= range);
    }

    private void DamageCreature(WorldEntityState creature, int amount)
    {
        creature.Durability = Math.Max(0, creature.Durability - amount);
        creature.AiAccumulator = 0f;
        creature.HitFlashSeconds = 0.16f;
        if (creature.Durability <= 0)
        {
            creature.Removed = true;
        }
    }

    private void DamagePlayer(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        runtime.Player.Survival.Damage(amount);
        runtime.WorldState.PlayerHitFlashSeconds = 0.18f;
    }

    private static string ShortName(ContentId id) => id.Value.Split(':')[1].Replace('_', ' ');
}
