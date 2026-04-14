using Downroot.Core.Definitions;
using Downroot.Core.Gameplay;
using Downroot.Core.Ids;

namespace Downroot.Gameplay.Runtime.Systems;

public sealed class CraftingSystem(GameRuntime runtime, WorldQueryService worldQuery)
{
    public IReadOnlyList<RecipeDef> GetRecipesForWorkspace(CraftWorkspaceMode workspaceMode)
    {
        return workspaceMode switch
        {
            CraftWorkspaceMode.Handcraft => runtime.Content.Recipes.All.Where(recipe => recipe.RequiredStationKind == CraftingStationKind.Handcraft).ToArray(),
            CraftWorkspaceMode.Workbench => runtime.Content.Recipes.All.Where(recipe => recipe.RequiredStationKind == CraftingStationKind.Workbench).ToArray(),
            CraftWorkspaceMode.Furnace => runtime.Content.Recipes.All.Where(recipe => recipe.RequiredStationKind == CraftingStationKind.Furnace).ToArray(),
            _ => []
        };
    }

    public bool Craft(ContentId recipeId) => TryCraft(recipeId, out _);

    public bool TryCraft(ContentId recipeId, out string failureReason)
    {
        var recipe = runtime.Content.Recipes.Get(recipeId);
        var outputs = GetRecipeOutputs(recipe);
        if (recipe.RequiredStationKind != CraftingStationKind.Handcraft && !IsStationAvailable(recipe.RequiredStationKind))
        {
            failureReason = recipe.RequiredStationKind == CraftingStationKind.Furnace
                ? $"{recipe.DisplayName} requires an active furnace."
                : $"{recipe.DisplayName} requires a nearby workbench.";
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

    public void UpdateFurnaceTask(float deltaSeconds)
    {
        var task = runtime.WorldState.ActiveFurnaceTask;
        if (task is null)
        {
            return;
        }

        var furnace = worldQuery.EnumerateActiveEntities().FirstOrDefault(entity => !entity.Removed && entity.Id == task.FurnaceEntityId);
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

    private bool IsStationAvailable(CraftingStationKind stationKind)
    {
        if (stationKind == CraftingStationKind.Handcraft)
        {
            return true;
        }

        return runtime.WorldState.ActiveStationKind == stationKind && runtime.WorldState.ActiveStationEntityId is not null;
    }

    private void PublishCraftResult(RecipeDef recipe, bool success, string message, StatusEventState statusEvent)
    {
        var prefix = success ? "[Craft]" : "[Craft][Blocked]";
        Console.WriteLine($"{prefix} {recipe.Id.Value}: {message}");
        runtime.WorldState.SetStatusEvent(statusEvent);
    }

    private static IReadOnlyList<ItemAmount> GetRecipeOutputs(RecipeDef recipe)
    {
        return recipe.ExtraResults is null
            ? [recipe.Result]
            : (new[] { recipe.Result }).Concat(recipe.ExtraResults).ToArray();
    }

    private static string ShortName(ContentId id) => id.Value.Split(':')[1].Replace('_', ' ');
}
