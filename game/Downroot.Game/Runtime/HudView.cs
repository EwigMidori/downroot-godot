using Downroot.Core.Ids;
using Downroot.UI.Presentation;
using Godot;

namespace Downroot.Game.Runtime;

public sealed partial class HudView : CanvasLayer
{
    public sealed record BarParts(Control BarRoot, Panel BarFrame, ColorRect BarTrack, ColorRect BarFill, float Width);

    public sealed record SlotParts(Control SlotRoot, Panel SlotBackground, Panel SelectionFrame, TextureRect ItemIcon, Label StackCountLabel);

    public sealed record RecipeRowParts(
        PanelContainer RowRoot,
        TextureRect RecipeResultIcon,
        Label RecipeNameLabel,
        HFlowContainer RecipeCostContainer,
        Button RecipeCraftButton,
        ColorRect RecipeUnavailableMask);

    public Control HudRoot { get; }
    public ColorRect NightOverlay { get; }
    public PanelContainer PlayerStatusPanel { get; }
    public Label TimeOfDayLabel { get; }
    public BarParts HealthBarWidget { get; }
    public BarParts HungerBarWidget { get; }
    public HBoxContainer HotbarSlotRow { get; }
    public IReadOnlyList<SlotParts> HotbarSlots { get; }
    public PanelContainer HotbarPanel { get; }
    public PanelContainer PrimaryHelpPanel { get; }
    public HFlowContainer HelpHintRow { get; }
    public PanelContainer StatusBanner { get; }
    public Label StatusMessageLabel { get; }
    public PanelContainer ContextPromptPanel { get; }
    public Label PromptKeyLabel { get; }
    public TextureRect PromptVerbIcon { get; }
    public Label PromptVerbLabel { get; }
    public Label PromptTargetLabel { get; }
    public PanelContainer DestroyProgressPanel { get; }
    public Label DestroyTargetLabel { get; }
    public BarParts DestroyProgressWidget { get; }
    public PanelContainer CraftWorkspacePanel { get; }
    public TextureRect CraftModeIcon { get; }
    public Label CraftModeLabel { get; }
    public ScrollContainer RecipeListScroll { get; }
    public VBoxContainer RecipeListContainer { get; }
    public Label CraftInventoryTitleLabel { get; }
    public GridContainer CraftInventoryGrid { get; }
    public IReadOnlyList<SlotParts> InventorySlots { get; }

    public HudView()
    {
        Name = "HudView";

        NightOverlay = new ColorRect
        {
            Name = "NightOverlay",
            Color = new Color(0.03f, 0.05f, 0.15f, 0f),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            AnchorRight = 1,
            AnchorBottom = 1
        };
        AddChild(NightOverlay);

        HudRoot = new Control
        {
            Name = "HudRoot",
            AnchorRight = 1,
            AnchorBottom = 1,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        AddChild(HudRoot);

        PlayerStatusPanel = CreatePanel("PlayerStatusPanel", new Vector2(16, 16), new Vector2(236, 92));
        HudRoot.AddChild(PlayerStatusPanel);
        var playerStatusStack = new VBoxContainer();
        PlayerStatusPanel.AddChild(playerStatusStack);

        TimeOfDayLabel = new Label { Name = "TimeOfDayLabel" };
        playerStatusStack.AddChild(TimeOfDayLabel);
        HealthBarWidget = CreateBar("HealthBarWidget", 208, new Color(0.74f, 0.18f, 0.22f));
        playerStatusStack.AddChild(HealthBarWidget.BarRoot);
        HungerBarWidget = CreateBar("HungerBarWidget", 208, new Color(0.88f, 0.7f, 0.18f));
        playerStatusStack.AddChild(HungerBarWidget.BarRoot);

        HotbarPanel = CreatePanel("HotbarPanel", Vector2.Zero, new Vector2(504, 72));
        HudRoot.AddChild(HotbarPanel);
        HotbarSlotRow = new HBoxContainer { Name = "HotbarSlotRow" };
        SetSeparation(HotbarSlotRow, 8);
        HotbarPanel.AddChild(HotbarSlotRow);
        HotbarSlots = Enumerable.Range(0, 8).Select(index =>
        {
            var slot = CreateSlot(index == 0 ? "HotbarSlotWidget" : $"HotbarSlotWidget{index + 1}", 52, true);
            HotbarSlotRow.AddChild(slot.SlotRoot);
            return slot;
        }).ToArray();

        PrimaryHelpPanel = CreatePanel("PrimaryHelpPanel", Vector2.Zero, new Vector2(280, 96));
        HudRoot.AddChild(PrimaryHelpPanel);
        HelpHintRow = new HFlowContainer { Name = "HelpHintRow" };
        SetFlowSeparation(HelpHintRow, 8, 8);
        PrimaryHelpPanel.AddChild(HelpHintRow);
        AddHelpHint("WASD", "Move");
        AddHelpHint("F", "Interact");
        AddHelpHint("E", "Craft");
        AddHelpHint("RMB", "Place");
        AddHelpHint("LMB Hold", "Break");

        StatusBanner = CreatePanel("StatusBanner", Vector2.Zero, new Vector2(336, 40));
        StatusBanner.Visible = false;
        HudRoot.AddChild(StatusBanner);
        StatusMessageLabel = new Label
        {
            Name = "StatusMessageLabel",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        StatusBanner.AddChild(StatusMessageLabel);

        ContextPromptPanel = CreatePanel("ContextPromptPanel", Vector2.Zero, new Vector2(324, 40));
        ContextPromptPanel.Visible = false;
        HudRoot.AddChild(ContextPromptPanel);
        var promptRow = new HBoxContainer();
        SetSeparation(promptRow, 10);
        ContextPromptPanel.AddChild(promptRow);
        PromptKeyLabel = CreateKeycapLabel("PromptKeyLabel", "F");
        promptRow.AddChild(WrapInPanel("PromptKeyPanel", PromptKeyLabel, new Color(0.16f, 0.18f, 0.22f), new Color(0.86f, 0.86f, 0.9f)));
        PromptVerbIcon = new TextureRect
        {
            Name = "PromptVerbIcon",
            CustomMinimumSize = new Vector2(18, 18),
            StretchMode = TextureRect.StretchModeEnum.KeepCentered
        };
        promptRow.AddChild(PromptVerbIcon);
        PromptVerbLabel = new Label { Name = "PromptVerbLabel" };
        promptRow.AddChild(PromptVerbLabel);
        PromptTargetLabel = new Label { Name = "PromptTargetLabel" };
        promptRow.AddChild(PromptTargetLabel);

        DestroyProgressPanel = CreateFloatingPanel("DestroyProgressPanel", new Vector2(160, 42));
        DestroyProgressPanel.Visible = false;
        HudRoot.AddChild(DestroyProgressPanel);
        var destroyStack = new VBoxContainer();
        SetSeparation(destroyStack, 4);
        DestroyProgressPanel.AddChild(destroyStack);
        DestroyTargetLabel = new Label
        {
            Name = "DestroyTargetLabel",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        destroyStack.AddChild(DestroyTargetLabel);
        DestroyProgressWidget = CreateBar("DestroyProgressWidget", 132, new Color(0.94f, 0.58f, 0.2f));
        destroyStack.AddChild(DestroyProgressWidget.BarRoot);

        CraftWorkspacePanel = CreatePanel("CraftWorkspacePanel", Vector2.Zero, new Vector2(364, 504));
        CraftWorkspacePanel.Visible = false;
        HudRoot.AddChild(CraftWorkspacePanel);
        var workspaceStack = new VBoxContainer();
        SetSeparation(workspaceStack, 12);
        CraftWorkspacePanel.AddChild(workspaceStack);

        var craftHeader = new HBoxContainer { Name = "CraftHeader" };
        SetSeparation(craftHeader, 10);
        workspaceStack.AddChild(craftHeader);
        CraftModeIcon = new TextureRect
        {
            Name = "CraftModeIcon",
            CustomMinimumSize = new Vector2(20, 20),
            StretchMode = TextureRect.StretchModeEnum.KeepCentered
        };
        craftHeader.AddChild(CraftModeIcon);
        CraftModeLabel = new Label { Name = "CraftModeLabel" };
        craftHeader.AddChild(CraftModeLabel);

        var craftBody = new VBoxContainer { Name = "CraftBody" };
        SetSeparation(craftBody, 12);
        workspaceStack.AddChild(craftBody);

        var recipeRegion = new VBoxContainer { Name = "RecipeListRegion" };
        SetSeparation(recipeRegion, 6);
        craftBody.AddChild(recipeRegion);
        RecipeListScroll = new ScrollContainer
        {
            Name = "RecipeListScroll",
            CustomMinimumSize = new Vector2(0, 260),
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        recipeRegion.AddChild(RecipeListScroll);
        RecipeListContainer = new VBoxContainer
        {
            Name = "RecipeListContainer",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        SetSeparation(RecipeListContainer, 8);
        RecipeListScroll.AddChild(RecipeListContainer);

        var inventoryRegion = new VBoxContainer { Name = "CraftInventoryRegion" };
        SetSeparation(inventoryRegion, 8);
        craftBody.AddChild(inventoryRegion);
        CraftInventoryTitleLabel = new Label
        {
            Name = "CraftInventoryTitleLabel",
            Text = "Inventory"
        };
        inventoryRegion.AddChild(CraftInventoryTitleLabel);
        CraftInventoryGrid = new GridContainer
        {
            Name = "CraftInventoryGrid",
            Columns = 4
        };
        inventoryRegion.AddChild(CraftInventoryGrid);
        InventorySlots = Enumerable.Range(0, 16).Select(index =>
        {
            var slot = CreateSlot(index == 0 ? "InventorySlotWidget" : $"InventorySlotWidget{index + 1}", 42, false);
            CraftInventoryGrid.AddChild(slot.SlotRoot);
            return slot;
        }).ToArray();
    }

    public void SetBarValue(BarParts parts, float percent)
    {
        var clamped = Mathf.Clamp(percent, 0f, 1f);
        parts.BarFill.Size = new Vector2(parts.Width * clamped, parts.BarFill.Size.Y);
    }

    public void SetSlot(SlotParts slot, Texture2D? icon, int quantity, bool selected)
    {
        slot.SelectionFrame.Visible = selected;
        slot.ItemIcon.Visible = icon is not null;
        slot.ItemIcon.Texture = icon;
        slot.StackCountLabel.Visible = quantity > 1;
        slot.StackCountLabel.Text = quantity > 1 ? quantity.ToString() : string.Empty;
    }

    public RecipeRowParts CreateRecipeRow(CraftRecipeViewData recipe, Action<ContentId> onCraft)
    {
        var rowRoot = new PanelContainer
        {
            Name = "CraftRecipeRow",
            CustomMinimumSize = new Vector2(0, 92),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        rowRoot.AddThemeStyleboxOverride("panel", CreatePanelStyle(new Color(0.14f, 0.16f, 0.2f, 0.95f), new Color(0.26f, 0.3f, 0.36f)));

        var content = new VBoxContainer
        {
            Name = "RecipeRowContent",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        SetSeparation(content, 8);
        rowRoot.AddChild(content);

        var topRow = new HBoxContainer
        {
            Name = "RecipeTopRow",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        SetSeparation(topRow, 10);
        content.AddChild(topRow);

        var resultIcon = new TextureRect
        {
            Name = "RecipeResultIcon",
            CustomMinimumSize = new Vector2(36, 36),
            StretchMode = TextureRect.StretchModeEnum.KeepCentered
        };
        topRow.AddChild(resultIcon);

        var nameLabel = new Label
        {
            Name = "RecipeNameLabel",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        topRow.AddChild(nameLabel);

        var craftButton = new Button
        {
            Name = "RecipeCraftButton",
            Text = "Craft",
            CustomMinimumSize = new Vector2(72, 32),
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        craftButton.Pressed += () => onCraft(recipe.RecipeId);
        topRow.AddChild(craftButton);

        var costContainer = new HFlowContainer
        {
            Name = "RecipeCostContainer",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        SetFlowSeparation(costContainer, 6, 6);
        content.AddChild(costContainer);

        var unavailableMask = new ColorRect
        {
            Name = "RecipeUnavailableMask",
            Color = new Color(0.03f, 0.04f, 0.05f, 0.45f),
            AnchorRight = 1,
            AnchorBottom = 1,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        rowRoot.AddChild(unavailableMask);

        return new RecipeRowParts(rowRoot, resultIcon, nameLabel, costContainer, craftButton, unavailableMask);
    }

    public Control CreateCostChip(RecipeCostViewData cost, Texture2D? icon)
    {
        var chip = new Panel
        {
            Name = "RecipeCostChip",
            CustomMinimumSize = new Vector2(54, 28),
            TooltipText = cost.IsSatisfied
                ? $"{cost.ItemName} x{cost.Amount}"
                : $"{cost.ItemName}: missing {cost.MissingAmount}"
        };
        // Keep recipe rows compact on smaller windows. Item names move to tooltip so cost chips stay icon-first.
        chip.AddThemeStyleboxOverride("panel", CreatePanelStyle(
            new Color(0.18f, 0.2f, 0.24f, 0.96f),
            new Color(0.25f, 0.28f, 0.34f)));

        var row = new HBoxContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
        SetSeparation(row, 4);
        chip.AddChild(row);

        row.AddChild(new TextureRect
        {
            Name = "CostItemIcon",
            Texture = icon,
            CustomMinimumSize = new Vector2(16, 16),
            StretchMode = TextureRect.StretchModeEnum.KeepCentered
        });

        row.AddChild(new Label
        {
            Name = "CostAmountLabel",
            Text = $"x{cost.Amount}",
            Modulate = cost.IsSatisfied ? new Color(0.74f, 0.92f, 0.74f) : new Color(0.96f, 0.62f, 0.62f)
        });

        return chip;
    }

    public Texture2D CreatePromptIcon(PromptIconKind kind)
    {
        return kind switch
        {
            PromptIconKind.Open => CreateIconTexture(new Color(0.43f, 0.8f, 0.96f), [new Rect2I(2, 6, 9, 4), new Rect2I(9, 3, 5, 10)]),
            PromptIconKind.Close => CreateIconTexture(new Color(0.94f, 0.46f, 0.42f), [new Rect2I(5, 6, 9, 4), new Rect2I(2, 3, 5, 10)]),
            PromptIconKind.Gather => CreateIconTexture(new Color(0.58f, 0.88f, 0.48f), [new Rect2I(7, 1, 2, 14), new Rect2I(3, 5, 10, 2)]),
            PromptIconKind.Eat => CreateIconTexture(new Color(0.98f, 0.76f, 0.22f), [new Rect2I(4, 4, 8, 8)]),
            PromptIconKind.PickUp => CreateIconTexture(new Color(0.86f, 0.82f, 0.4f), [new Rect2I(7, 2, 2, 10), new Rect2I(4, 2, 8, 2)]),
            _ => CreateIconTexture(new Color(0.72f, 0.74f, 0.96f), [new Rect2I(3, 3, 10, 10)])
        };
    }

    public Texture2D CreateCraftModeIcon(CraftModeIconKind kind)
    {
        return kind switch
        {
            CraftModeIconKind.Workbench => CreateIconTexture(new Color(0.48f, 0.78f, 0.92f), [new Rect2I(2, 8, 12, 3), new Rect2I(4, 4, 8, 3), new Rect2I(3, 11, 2, 3), new Rect2I(11, 11, 2, 3)]),
            _ => CreateIconTexture(new Color(0.96f, 0.72f, 0.28f), [new Rect2I(4, 3, 8, 3), new Rect2I(2, 6, 12, 3), new Rect2I(4, 9, 8, 4)])
        };
    }

    private void AddHelpHint(string key, string action)
    {
        var chip = new PanelContainer { Name = "HelpHintChip" };
        chip.AddThemeStyleboxOverride("panel", CreatePanelStyle(new Color(0.12f, 0.14f, 0.18f, 0.94f), new Color(0.25f, 0.28f, 0.34f)));
        var row = new HBoxContainer();
        SetSeparation(row, 6);
        chip.AddChild(row);
        row.AddChild(WrapInPanel("KeyPanel", CreateKeycapLabel("KeyLabel", key), new Color(0.18f, 0.2f, 0.24f), new Color(0.76f, 0.78f, 0.84f)));
        row.AddChild(new Label
        {
            Name = "ActionLabel",
            Text = action
        });
        HelpHintRow.AddChild(chip);
    }

    private static SlotParts CreateSlot(string name, float size, bool includeSelectionFrame)
    {
        var slotRoot = new Control
        {
            Name = name,
            CustomMinimumSize = new Vector2(size, size)
        };

        var background = new Panel
        {
            Name = "SlotBackground",
            AnchorRight = 1,
            AnchorBottom = 1
        };
        background.AddThemeStyleboxOverride("panel", CreatePanelStyle(new Color(0.11f, 0.13f, 0.17f, 0.94f), new Color(0.28f, 0.31f, 0.38f)));
        slotRoot.AddChild(background);

        var selectionFrame = new Panel
        {
            Name = "SelectionFrame",
            AnchorRight = 1,
            AnchorBottom = 1,
            Visible = includeSelectionFrame
        };
        selectionFrame.AddThemeStyleboxOverride("panel", CreateOutlineStyle(new Color(0.97f, 0.82f, 0.28f), 3));
        selectionFrame.Visible = false;
        slotRoot.AddChild(selectionFrame);

        var itemIcon = new TextureRect
        {
            Name = "ItemIcon",
            AnchorRight = 1,
            AnchorBottom = 1,
            OffsetLeft = 6,
            OffsetTop = 6,
            OffsetRight = -6,
            OffsetBottom = -6,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            Visible = false
        };
        slotRoot.AddChild(itemIcon);

        var stackCountLabel = new Label
        {
            Name = "StackCountLabel",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            AnchorRight = 1,
            AnchorBottom = 1,
            OffsetRight = -4,
            OffsetBottom = -2,
            Visible = false
        };
        slotRoot.AddChild(stackCountLabel);

        return new SlotParts(slotRoot, background, selectionFrame, itemIcon, stackCountLabel);
    }

    private static BarParts CreateBar(string name, float width, Color fillColor)
    {
        var root = new Control
        {
            Name = "BarRoot",
            CustomMinimumSize = new Vector2(width, 14)
        };
        var frame = new Panel
        {
            Name = "BarFrame",
            AnchorRight = 1,
            AnchorBottom = 1
        };
        frame.AddThemeStyleboxOverride("panel", CreateOutlineStyle(new Color(0.22f, 0.24f, 0.3f), 1));
        root.AddChild(frame);

        var track = new ColorRect
        {
            Name = "BarTrack",
            Color = new Color(0.08f, 0.09f, 0.12f),
            Position = new Vector2(2, 2),
            Size = new Vector2(width - 4, 10)
        };
        frame.AddChild(track);

        var fill = new ColorRect
        {
            Name = "BarFill",
            Color = fillColor,
            Size = new Vector2(0, 10)
        };
        track.AddChild(fill);

        root.Name = name;
        return new BarParts(root, frame, track, fill, width - 4);
    }

    private static PanelContainer CreatePanel(string name, Vector2 position, Vector2 size)
    {
        var panel = new PanelContainer
        {
            Name = name,
            Position = position,
            Size = size
        };
        panel.AddThemeStyleboxOverride("panel", CreatePanelStyle(new Color(0.09f, 0.11f, 0.15f, 0.92f), new Color(0.24f, 0.28f, 0.34f)));
        return panel;
    }

    private static PanelContainer CreateFloatingPanel(string name, Vector2 size)
    {
        var panel = new PanelContainer
        {
            Name = name,
            Size = size
        };
        panel.AddThemeStyleboxOverride("panel", CreatePanelStyle(new Color(0.09f, 0.11f, 0.15f, 0.96f), new Color(0.26f, 0.3f, 0.36f)));
        return panel;
    }

    private static Control WrapInPanel(string name, Label label, Color background, Color border)
    {
        var panel = new PanelContainer { Name = name };
        panel.AddThemeStyleboxOverride("panel", CreatePanelStyle(background, border));
        panel.AddChild(label);
        return panel;
    }

    private static Label CreateKeycapLabel(string name, string text)
    {
        return new Label
        {
            Name = name,
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Center
        };
    }

    private static StyleBoxFlat CreatePanelStyle(Color background, Color border)
    {
        return new StyleBoxFlat
        {
            BgColor = background,
            BorderColor = border,
            BorderWidthBottom = 1,
            BorderWidthTop = 1,
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            ContentMarginBottom = 8,
            ContentMarginTop = 8,
            ContentMarginLeft = 8,
            ContentMarginRight = 8
        };
    }

    private static StyleBoxFlat CreateOutlineStyle(Color border, int width)
    {
        return new StyleBoxFlat
        {
            BgColor = new Color(0f, 0f, 0f, 0f),
            BorderColor = border,
            BorderWidthBottom = width,
            BorderWidthTop = width,
            BorderWidthLeft = width,
            BorderWidthRight = width,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8
        };
    }

    private static Texture2D CreateIconTexture(Color color, Rect2I[] blocks)
    {
        var image = Image.CreateEmpty(16, 16, false, Image.Format.Rgba8);
        image.Fill(new Color(0, 0, 0, 0));
        foreach (var block in blocks)
        {
            image.FillRect(block, color);
        }

        return ImageTexture.CreateFromImage(image);
    }

    private static void SetSeparation(BoxContainer container, int separation)
    {
        container.AddThemeConstantOverride("separation", separation);
    }

    private static void SetFlowSeparation(FlowContainer container, int horizontal, int vertical)
    {
        container.AddThemeConstantOverride("h_separation", horizontal);
        container.AddThemeConstantOverride("v_separation", vertical);
    }
}
