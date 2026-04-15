using Downroot.UI.Presentation;
using Godot;

namespace Downroot.Game.Runtime;

public sealed class LoadGameController
{
    public event Action<string>? LoadRequested;
    public event Action<string>? DeleteRequested;
    public event Action? BackRequested;

    private readonly Control _root;
    private readonly ItemList _slotList;
    private readonly RichTextLabel _details;
    private readonly RichTextLabel _errorLabel;
    private readonly Button _loadButton;
    private readonly Button _deleteButton;
    private IReadOnlyList<SaveSlotViewData> _slots = [];

    public LoadGameController()
    {
        _root = new Control { AnchorRight = 1, AnchorBottom = 1 };
        _root.AddChild(new ColorRect { Color = new Color(0.07f, 0.09f, 0.12f), AnchorRight = 1, AnchorBottom = 1 });

        var body = new HBoxContainer
        {
            AnchorLeft = 0.5f,
            AnchorTop = 0.5f,
            AnchorRight = 0.5f,
            AnchorBottom = 0.5f,
            OffsetLeft = -360,
            OffsetTop = -180,
            OffsetRight = 360,
            OffsetBottom = 180
        };
        body.AddThemeConstantOverride("separation", 16);
        _root.AddChild(body);

        _slotList = new ItemList
        {
            CustomMinimumSize = new Vector2(260, 280),
            SelectMode = ItemList.SelectModeEnum.Single,
            FocusMode = Control.FocusModeEnum.None
        };
        _slotList.ItemSelected += _ => RefreshDetails();
        body.AddChild(_slotList);

        _details = new RichTextLabel
        {
            CustomMinimumSize = new Vector2(380, 280),
            BbcodeEnabled = true,
            FitContent = true
        };
        body.AddChild(_details);

        _errorLabel = new RichTextLabel
        {
            AnchorLeft = 0.5f,
            AnchorTop = 1,
            AnchorRight = 0.5f,
            AnchorBottom = 1,
            OffsetLeft = -360,
            OffsetTop = -96,
            OffsetRight = 360,
            OffsetBottom = -66,
            BbcodeEnabled = true,
            FitContent = true
        };
        _root.AddChild(_errorLabel);

        var buttons = new HBoxContainer
        {
            AnchorLeft = 0.5f,
            AnchorTop = 1,
            AnchorRight = 0.5f,
            AnchorBottom = 1,
            OffsetLeft = -220,
            OffsetTop = -64,
            OffsetRight = 220,
            OffsetBottom = -20
        };
        buttons.AddThemeConstantOverride("separation", 10);
        _root.AddChild(buttons);

        _loadButton = new Button { Text = "Load", FocusMode = Control.FocusModeEnum.None };
        _loadButton.Pressed += () =>
        {
            if (TryGetSelectedSlot(out var slot))
            {
                LoadRequested?.Invoke(slot.SlotId);
            }
        };
        _deleteButton = new Button { Text = "Delete", FocusMode = Control.FocusModeEnum.None };
        _deleteButton.Pressed += () =>
        {
            if (TryGetSelectedSlot(out var slot))
            {
                DeleteRequested?.Invoke(slot.SlotId);
            }
        };
        var back = new Button { Text = "Back", FocusMode = Control.FocusModeEnum.None };
        back.Pressed += () => BackRequested?.Invoke();
        buttons.AddChild(_loadButton);
        buttons.AddChild(_deleteButton);
        buttons.AddChild(back);
    }

    public Control View => _root;

    public void Bind(IReadOnlyList<SaveSlotViewData> slots, string? errorMessage = null)
    {
        _slots = slots;
        _slotList.Clear();
        foreach (var slot in slots)
        {
            _slotList.AddItem(slot.DisplayName);
        }

        if (slots.Count > 0)
        {
            _slotList.Select(0);
        }

        _errorLabel.Text = string.IsNullOrWhiteSpace(errorMessage)
            ? string.Empty
            : $"[color=#ff8e8e]{errorMessage}[/color]";
        RefreshDetails();
    }

    private void RefreshDetails()
    {
        if (!TryGetSelectedSlot(out var slot))
        {
            _details.Text = "No save selected.";
            _loadButton.Disabled = true;
            _deleteButton.Disabled = true;
            return;
        }

        _loadButton.Disabled = false;
        _deleteButton.Disabled = false;
        _details.Text =
            $"[b]{slot.DisplayName}[/b]\n" +
            $"Seed: {slot.WorldSeed}\n" +
            $"Last Save: {slot.LastWriteUtc.LocalDateTime:yyyy-MM-dd HH:mm:ss}\n" +
            $"Enabled Mods: {(slot.EnabledPackIds.Count == 0 ? "basegame" : string.Join(", ", slot.EnabledPackIds))}\n" +
            $"Health: {slot.PlayerHealth}\n" +
            $"Hunger: {slot.PlayerHunger}\n" +
            $"World: {slot.CurrentWorldSpace}";
    }

    private bool TryGetSelectedSlot(out SaveSlotViewData slot)
    {
        var selected = _slotList.GetSelectedItems();
        if (selected.Length == 0 || selected[0] < 0 || selected[0] >= _slots.Count)
        {
            slot = null!;
            return false;
        }

        slot = _slots[selected[0]];
        return true;
    }
}
