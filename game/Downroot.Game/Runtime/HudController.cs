using Downroot.Core.Ids;
using Downroot.Game.Infrastructure;
using Downroot.Gameplay.Runtime;
using Downroot.UI.Presentation;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Downroot.Game.Runtime;

public sealed class HudController
{
    private readonly Node _host;
    private readonly TextureContentLoader _textureLoader;
    private readonly GamePresentationBuilder _builder = new();
    private readonly HudLayoutResolver _layoutResolver = new();
    private readonly Dictionary<string, Texture2D> _itemIconCache = [];
    private readonly HudView _view = new();
    private GameSimulation? _simulation;
    private string? _recipeStateKey;

    public HudController(Node host, TextureContentLoader textureLoader)
    {
        _host = host;
        _textureLoader = textureLoader;
    }

    public HudView View => _view;

    public void Initialize(GameSimulation simulation)
    {
        _simulation = simulation;
        _host.AddChild(_view);
    }

    public void Refresh(GameRuntime runtime, Func<NumericsVector2, Vector2> worldToScreen)
    {
        var snapshot = _builder.Build(runtime, _simulation!);

        _view.TimeOfDayLabel.Text = snapshot.HudStatus.TimeOfDayLabel;
        _view.NightOverlay.Color = new Color(0.03f, 0.05f, 0.15f, snapshot.HudStatus.IsNight ? 0.32f : 0f);
        _view.SetBarValue(_view.HealthBarWidget, snapshot.HudStatus.HealthPercent);
        _view.SetBarValue(_view.HungerBarWidget, snapshot.HudStatus.HungerPercent);

        for (var index = 0; index < _view.HotbarSlots.Count; index++)
        {
            var slotView = snapshot.HotbarSlots[index];
            _view.SetSlot(_view.HotbarSlots[index], ResolveItemIcon(slotView.ItemId, runtime), slotView.Quantity, slotView.IsSelected);
        }

        _view.CraftWorkspacePanel.Visible = snapshot.CraftingPanel.IsVisible;
        _view.CraftModeLabel.Text = snapshot.CraftingPanel.CraftModeLabel;
        _view.CraftModeIcon.Texture = _view.CreateCraftModeIcon(snapshot.CraftingPanel.CraftModeIcon);

        for (var index = 0; index < _view.InventorySlots.Count; index++)
        {
            var slotView = snapshot.CraftingPanel.InventorySlots[index];
            _view.SetSlot(_view.InventorySlots[index], ResolveItemIcon(slotView.ItemId, runtime), slotView.Quantity, false);
        }

        var recipeStateKey = string.Join('|', new[]
        {
            snapshot.CraftingPanel.CraftModeLabel,
            string.Join(',', snapshot.CraftingPanel.Recipes.Select(recipe => $"{recipe.RecipeId.Value}:{recipe.CanCraft}"))
        });

        if (_recipeStateKey != recipeStateKey)
        {
            RebuildRecipeList(snapshot.CraftingPanel, runtime);
            _recipeStateKey = recipeStateKey;
        }

        _layoutResolver.Apply(_view, _host.GetViewport().GetVisibleRect().Size);

        _view.ContextPromptPanel.Visible = snapshot.InteractionPrompt.IsVisible;
        _view.PromptKeyLabel.Text = snapshot.InteractionPrompt.PromptKeyLabel;
        _view.PromptVerbIcon.Texture = _view.CreatePromptIcon(snapshot.InteractionPrompt.PromptIconKind);
        _view.PromptVerbLabel.Text = snapshot.InteractionPrompt.PromptVerbLabel;
        _view.PromptTargetLabel.Text = snapshot.InteractionPrompt.PromptTargetLabel;

        _view.StatusBanner.Visible = snapshot.StatusBanner.IsVisible;
        _view.StatusMessageLabel.Text = snapshot.StatusBanner.StatusMessageLabel;

        _view.DestroyProgressPanel.Visible = snapshot.DestroyProgress.IsVisible;
        if (snapshot.DestroyProgress.IsVisible)
        {
            _view.DestroyTargetLabel.Text = snapshot.DestroyProgress.DestroyTargetLabel;
            _view.SetBarValue(_view.DestroyProgressWidget, snapshot.DestroyProgress.Progress01);
            var screenPosition = worldToScreen(snapshot.DestroyProgress.WorldPosition);
            _view.DestroyProgressPanel.Position = _layoutResolver.ResolveDestroyPanelPosition(
                _view,
                _host.GetViewport().GetVisibleRect().Size,
                screenPosition);
        }
    }

    private void RebuildRecipeList(CraftingPanelViewData panelViewData, GameRuntime runtime)
    {
        foreach (var child in _view.RecipeListContainer.GetChildren())
        {
            child.QueueFree();
        }

        if (!panelViewData.IsVisible)
        {
            return;
        }

        foreach (var recipe in panelViewData.Recipes)
        {
            var row = _view.CreateRecipeRow(recipe, OnCraftRequested);
            row.RecipeResultIcon.Texture = ResolveItemIcon(recipe.ResultItemId, runtime);
            row.RecipeNameLabel.Text = recipe.RecipeName;
            row.RecipeNameLabel.Modulate = recipe.CanCraft ? Colors.White : new Color(0.72f, 0.72f, 0.72f);
            row.RecipeCraftButton.Disabled = !recipe.CanCraft;
            row.RecipeUnavailableMask.Visible = !recipe.CanCraft;

            foreach (var cost in recipe.Costs)
            {
                row.RecipeCostContainer.AddChild(_view.CreateCostChip(cost, ResolveItemIcon(cost.ItemId, runtime)));
            }

            _view.RecipeListContainer.AddChild(row.RowRoot);
        }
    }

    private Texture2D? ResolveItemIcon(ContentId? itemId, GameRuntime runtime)
    {
        if (itemId is null)
        {
            return null;
        }

        var key = itemId.Value.Value;
        if (_itemIconCache.TryGetValue(key, out var texture))
        {
            return texture;
        }

        texture = _textureLoader.LoadItem(runtime.Content.Items.Get(itemId.Value)).Texture;
        _itemIconCache[key] = texture;
        return texture;
    }

    private void OnCraftRequested(ContentId recipeId)
    {
        GD.Print($"[Craft UI] Clicked {recipeId.Value}");
        if (!_simulation!.TryCraft(recipeId, out var failureReason))
        {
            GD.Print($"[Craft UI] Blocked {recipeId.Value}: {failureReason}");
        }

        _recipeStateKey = null;
    }
}
