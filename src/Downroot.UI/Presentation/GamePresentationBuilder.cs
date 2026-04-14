using Downroot.Content.Registries;
using Downroot.Core.Definitions;
using Downroot.Core.Gameplay;
using Downroot.Core.Ids;
using Downroot.Gameplay.Runtime;

namespace Downroot.UI.Presentation;

public sealed class GamePresentationBuilder
{
    public GamePresentationSnapshot Build(GameRuntime runtime, GameSimulation simulation)
    {
        return new GamePresentationSnapshot(
            BuildHudStatus(runtime),
            BuildHotbar(runtime),
            BuildCraftingPanel(runtime, simulation),
            BuildInteractionPrompt(runtime),
            BuildStatusBanner(runtime),
            BuildDestroyProgress(runtime));
    }

    public HudStatusViewData BuildHudStatus(GameRuntime runtime)
    {
        var isNight = runtime.WorldState.IsNight(runtime.BootstrapConfig.DayLengthSeconds);
        return new HudStatusViewData(
            isNight ? "Night" : "Daytime",
            isNight,
            ToPercent(runtime.Player.Survival.Health, runtime.Player.Survival.MaxHealth),
            ToPercent(runtime.Player.Survival.Hunger, runtime.Player.Survival.MaxHunger),
            Math.Clamp(runtime.WorldState.PlayerHitFlashSeconds / 0.18f, 0f, 1f) * 0.45f);
    }

    public IReadOnlyList<HotbarSlotViewData> BuildHotbar(GameRuntime runtime)
    {
        return runtime.Player.Inventory.Slots
            .Take(runtime.Player.HotbarSize)
            .Select((slot, index) => new HotbarSlotViewData(slot.ItemId, slot.Quantity, index == runtime.Player.SelectedHotbarIndex))
            .ToArray();
    }

    public CraftingPanelViewData BuildCraftingPanel(GameRuntime runtime, GameSimulation simulation)
    {
        var mode = runtime.WorldState.WorkspaceMode;
        return new CraftingPanelViewData(
            mode != CraftWorkspaceMode.Hidden,
            mode switch
            {
                CraftWorkspaceMode.Furnace => "Furnace",
                CraftWorkspaceMode.Workbench => "Workbench",
                _ => "Handcraft"
            },
            mode switch
            {
                CraftWorkspaceMode.Furnace => CraftModeIconKind.Furnace,
                CraftWorkspaceMode.Workbench => CraftModeIconKind.Workbench,
                _ => CraftModeIconKind.Handcraft
            },
            mode == CraftWorkspaceMode.Hidden ? [] : BuildRecipeRows(runtime, simulation, mode),
            runtime.Player.Inventory.Slots
                .Take(16)
                .Select(slot => new InventorySlotViewData(slot.ItemId, slot.Quantity))
                .ToArray());
    }

    public IReadOnlyList<CraftRecipeViewData> BuildRecipeRows(GameRuntime runtime, GameSimulation simulation, CraftWorkspaceMode mode)
    {
        var activeTask = runtime.WorldState.ActiveFurnaceTask;
        return simulation.GetRecipesForWorkspace(mode)
            .Select(recipe =>
            {
                var costs = recipe.Ingredients
                    .Select(ingredient =>
                    {
                        var ownedAmount = runtime.Player.Inventory.Count(ingredient.ItemId);
                        var missingAmount = Math.Max(0, ingredient.Amount - ownedAmount);
                        return new RecipeCostViewData(
                            ingredient.ItemId,
                            ResolveItemName(runtime.Content, ingredient.ItemId),
                            ingredient.Amount,
                            missingAmount == 0,
                            missingAmount);
                    })
                    .ToArray();

                var outputs = recipe.ExtraResults is null
                    ? new[] { recipe.Result }
                    : (new[] { recipe.Result }).Concat(recipe.ExtraResults).ToArray();
                var isRunning = activeTask?.RecipeId == recipe.Id;
                var canCraft = recipe.Ingredients.All(ingredient => runtime.Player.Inventory.Has(ingredient.ItemId, ingredient.Amount))
                    && runtime.Player.Inventory.CanAddMany(outputs, runtime.Content)
                    && (!IsFurnaceRecipe(recipe) || activeTask is null || isRunning);

                return new CraftRecipeViewData(
                    recipe.Id,
                    recipe.Result.ItemId,
                    recipe.DisplayName,
                    costs,
                    canCraft,
                    isRunning ? "Busy" : IsFurnaceRecipe(recipe) ? "Smelt" : "Craft",
                    isRunning,
                    isRunning ? activeTask!.Progress01 : 0f);
            })
            .ToArray();
    }

    public InteractionPromptViewData BuildInteractionPrompt(GameRuntime runtime)
    {
        var context = runtime.WorldState.CurrentInteraction;
        if (context is null)
        {
            return new InteractionPromptViewData(false, "F", PromptIconKind.Use, string.Empty, string.Empty);
        }

        return new InteractionPromptViewData(
            true,
            "F",
            context.Verb switch
            {
                InteractionVerb.Open => PromptIconKind.Open,
                InteractionVerb.Close => PromptIconKind.Close,
                InteractionVerb.Gather => PromptIconKind.Gather,
                InteractionVerb.Eat => PromptIconKind.Eat,
                InteractionVerb.PickUp => PromptIconKind.PickUp,
                _ => PromptIconKind.Use
            },
            context.Verb switch
            {
                InteractionVerb.Open => "Open",
                InteractionVerb.Close => "Close",
                InteractionVerb.Gather => "Gather",
                InteractionVerb.Eat => "Eat",
                InteractionVerb.PickUp => "Pick Up",
                _ => "Use"
            },
            ResolveTargetName(runtime.Content, context.EntityKind, context.ContentId));
    }

    public StatusBannerViewData BuildStatusBanner(GameRuntime runtime)
    {
        if (runtime.WorldState.ActiveStatusEvent is null)
        {
            return new StatusBannerViewData(false, string.Empty);
        }

        var statusEvent = runtime.WorldState.ActiveStatusEvent;
        return runtime.WorldState.ActiveStatusEvent switch
        {
            { Kind: StatusEventKind.CraftedItem } => new StatusBannerViewData(true, $"Crafted {ResolveItemName(runtime.Content, statusEvent.PrimaryContentId!.Value)}"),
            { Kind: StatusEventKind.SmeltingStarted } => new StatusBannerViewData(true, $"Smelting {ResolveItemName(runtime.Content, statusEvent.PrimaryContentId!.Value)}"),
            { Kind: StatusEventKind.SmeltingCompleted } => new StatusBannerViewData(true, $"Smelted {ResolveItemName(runtime.Content, statusEvent.PrimaryContentId!.Value)}"),
            { Kind: StatusEventKind.MissingIngredient } => new StatusBannerViewData(true, $"Need {ResolveItemName(runtime.Content, statusEvent.PrimaryContentId!.Value)} x{statusEvent.Amount}"),
            { Kind: StatusEventKind.StationRequired } => new StatusBannerViewData(true, "Need a nearby station"),
            { Kind: StatusEventKind.InventoryFull } => new StatusBannerViewData(true, "Inventory full"),
            { Kind: StatusEventKind.EnteredPortal } => new StatusBannerViewData(true, "Entering Portal"),
            { Kind: StatusEventKind.ReturnedThroughPortal } => new StatusBannerViewData(true, "Returned to Overworld"),
            _ => new StatusBannerViewData(true, "Craft failed")
        };
    }

    public DestroyProgressViewData BuildDestroyProgress(GameRuntime runtime)
    {
        return runtime.WorldState.ActiveDestroyProgress switch
        {
            null => new DestroyProgressViewData(false, string.Empty, 0f, default),
            var progress => new DestroyProgressViewData(
                true,
                progress.IsRaisedFeature
                    ? runtime.Content.RaisedFeatures.Get(progress.ContentId).DisplayName
                    : ResolveTargetName(runtime.Content, progress.EntityKind!.Value, progress.ContentId),
                progress.Progress01,
                progress.WorldPosition)
        };
    }

    private static string ResolveTargetName(ContentRegistrySet content, WorldEntityKind entityKind, ContentId contentId)
    {
        return entityKind switch
        {
            WorldEntityKind.ResourceNode => content.ResourceNodes.Get(contentId).DisplayName,
            WorldEntityKind.Placeable => content.Placeables.Get(contentId).DisplayName,
            WorldEntityKind.ItemDrop => content.Items.Get(contentId).DisplayName,
            WorldEntityKind.Creature => content.Creatures.Get(contentId).DisplayName,
            _ => contentId.Value
        };
    }

    private static string ResolveItemName(ContentRegistrySet content, ContentId contentId) => content.Items.Get(contentId).DisplayName;

    private static bool IsFurnaceRecipe(RecipeDef recipe) => recipe.RequiredStationKind == CraftingStationKind.Furnace;

    private static float ToPercent(int current, int max) => max <= 0 ? 0f : Math.Clamp((float)current / max, 0f, 1f);
}
