using Downroot.Content.Packs;
using Downroot.Core.Content;
using Downroot.Core.Save;
using Downroot.Game.Infrastructure;
using Downroot.UI.Presentation;
using Godot;

namespace Downroot.Game.Runtime;

public sealed class ModManagementController
{
    public event Action? BackRequested;

    private readonly ModSettingsRepository _repository;
    private readonly ContentPackLocator _locator = new();
    private readonly ContentPackResolver _resolver = new();
    private readonly Control _root;
    private readonly ItemList _packList;
    private readonly RichTextLabel _details;
    private readonly RichTextLabel _errorLabel;
    private readonly Button _toggleButton;
    private readonly Button _applyButton;
    private readonly Dictionary<string, bool> _enabledStates = new(StringComparer.Ordinal);
    private IReadOnlyList<IContentPack> _packs = [];

    public ModManagementController(ModSettingsRepository repository)
    {
        _repository = repository;
        _root = new Control { AnchorRight = 1, AnchorBottom = 1 };
        _root.AddChild(new ColorRect { Color = new Color(0.07f, 0.09f, 0.12f), AnchorRight = 1, AnchorBottom = 1 });

        var body = new HBoxContainer
        {
            AnchorLeft = 0.5f,
            AnchorTop = 0.5f,
            AnchorRight = 0.5f,
            AnchorBottom = 0.5f,
            OffsetLeft = -430,
            OffsetTop = -220,
            OffsetRight = 430,
            OffsetBottom = 190
        };
        body.AddThemeConstantOverride("separation", 16);
        _root.AddChild(body);

        _packList = new ItemList
        {
            CustomMinimumSize = new Vector2(320, 340),
            SelectMode = ItemList.SelectModeEnum.Single,
            FocusMode = Control.FocusModeEnum.None
        };
        _packList.ItemSelected += _ => RefreshSelection();
        body.AddChild(_packList);

        var right = new VBoxContainer();
        right.AddThemeConstantOverride("separation", 10);
        body.AddChild(right);

        _details = new RichTextLabel
        {
            CustomMinimumSize = new Vector2(440, 280),
            BbcodeEnabled = true,
            FitContent = true
        };
        right.AddChild(_details);

        _errorLabel = new RichTextLabel
        {
            CustomMinimumSize = new Vector2(440, 52),
            BbcodeEnabled = true,
            FitContent = true
        };
        right.AddChild(_errorLabel);

        _toggleButton = new Button { Text = "Toggle", FocusMode = Control.FocusModeEnum.None };
        _toggleButton.Pressed += ToggleSelected;
        right.AddChild(_toggleButton);

        var buttons = new HBoxContainer
        {
            AnchorLeft = 0.5f,
            AnchorTop = 1,
            AnchorRight = 0.5f,
            AnchorBottom = 1,
            OffsetLeft = -160,
            OffsetTop = -58,
            OffsetRight = 160,
            OffsetBottom = -20
        };
        buttons.AddThemeConstantOverride("separation", 10);
        _root.AddChild(buttons);

        _applyButton = new Button { Text = "Apply", FocusMode = Control.FocusModeEnum.None };
        _applyButton.Pressed += Apply;
        var back = new Button { Text = "Back", FocusMode = Control.FocusModeEnum.None };
        back.Pressed += () => BackRequested?.Invoke();
        buttons.AddChild(_applyButton);
        buttons.AddChild(back);
    }

    public Control View => _root;

    public void Bind()
    {
        _packs = _locator.LocatePacks().OrderBy(pack => pack.PackId, StringComparer.Ordinal).ToArray();
        _enabledStates.Clear();
        foreach (var packId in _repository.Load().EnabledPackIds)
        {
            _enabledStates[packId] = true;
        }

        _enabledStates[BaseGameContentPack.Id] = true;
        RefreshList();
        if (_packList.ItemCount > 0)
        {
            _packList.Select(0);
        }

        _errorLabel.Text = string.Empty;
        RefreshSelection();
    }

    private void RefreshList()
    {
        _packList.Clear();
        foreach (var pack in _packs)
        {
            var enabled = _enabledStates.TryGetValue(pack.PackId, out var state) && state;
            var prefix = enabled ? "[x]" : "[ ]";
            _packList.AddItem($"{prefix} {pack.Manifest.DisplayName}  {pack.PackId}  v{pack.Manifest.Version}");
        }
    }

    private void RefreshSelection()
    {
        if (!TryGetSelectedPack(out var pack))
        {
            _details.Text = "No pack selected.";
            _toggleButton.Disabled = true;
            return;
        }

        var enabled = _enabledStates.TryGetValue(pack.PackId, out var state) && state;
        var dependents = GetEnabledDependents(pack.PackId);
        var canToggle = !string.Equals(pack.PackId, BaseGameContentPack.Id, StringComparison.Ordinal)
            && (enabled ? dependents.Count == 0 : true);

        _toggleButton.Disabled = !canToggle;
        _toggleButton.Text = enabled ? "Disable" : "Enable";

        var status = string.Equals(pack.PackId, BaseGameContentPack.Id, StringComparison.Ordinal)
            ? "Required"
            : enabled
                ? "Enabled"
                : "Disabled";
        if (dependents.Count > 0)
        {
            status += $" | Required by: {string.Join(", ", dependents)}";
        }

        var dependencyText = pack.Manifest.Dependencies.Count == 0
            ? "None"
            : string.Join(", ", pack.Manifest.Dependencies);
        _details.Text =
            $"[b]{pack.Manifest.DisplayName}[/b]\n" +
            $"PackId: {pack.PackId}\n" +
            $"Version: {pack.Manifest.Version}\n" +
            $"Built In: {(pack.Manifest.IsBuiltIn ? "Yes" : "No")}\n" +
            $"Description: {pack.Manifest.Description}\n" +
            $"Dependencies: {dependencyText}\n" +
            $"Status: {status}";
    }

    private void ToggleSelected()
    {
        if (!TryGetSelectedPack(out var pack))
        {
            return;
        }

        if (string.Equals(pack.PackId, BaseGameContentPack.Id, StringComparison.Ordinal))
        {
            _errorLabel.Text = "[color=#ffd479]basegame cannot be disabled.[/color]";
            return;
        }

        var enabled = _enabledStates.TryGetValue(pack.PackId, out var state) && state;
        if (enabled)
        {
            var dependents = GetEnabledDependents(pack.PackId);
            if (dependents.Count > 0)
            {
                _errorLabel.Text = $"[color=#ff8e8e]Cannot disable {pack.PackId}. Enabled packs depend on it: {string.Join(", ", dependents)}[/color]";
                RefreshSelection();
                return;
            }
        }

        _enabledStates[pack.PackId] = !enabled;
        _errorLabel.Text = string.Empty;
        RefreshList();
        Reselect(pack.PackId);
    }

    private void Apply()
    {
        var enabledPackIds = _enabledStates.Where(pair => pair.Value).Select(pair => pair.Key).ToArray();
        try
        {
            var resolved = _resolver.Resolve(enabledPackIds);
            _repository.Save(new GameModSettingsData
            {
                EnabledPackIds = resolved.OrderedPacks.Select(pack => pack.PackId).ToArray()
            });
            _errorLabel.Text = "[color=#a9f0c1]Applied mod selection.[/color]";
            Bind();
        }
        catch (Exception ex)
        {
            _errorLabel.Text = $"[color=#ff8e8e]{ex.Message}[/color]";
        }
    }

    private IReadOnlyList<string> GetEnabledDependents(string packId)
    {
        return _packs
            .Where(pack =>
                !string.Equals(pack.PackId, packId, StringComparison.Ordinal)
                && _enabledStates.TryGetValue(pack.PackId, out var enabled)
                && enabled
                && pack.Manifest.Dependencies.Contains(packId, StringComparer.Ordinal))
            .Select(pack => pack.PackId)
            .ToArray();
    }

    private bool TryGetSelectedPack(out IContentPack pack)
    {
        var selected = _packList.GetSelectedItems();
        if (selected.Length == 0 || selected[0] < 0 || selected[0] >= _packs.Count)
        {
            pack = null!;
            return false;
        }

        pack = _packs[selected[0]];
        return true;
    }

    private void Reselect(string packId)
    {
        for (var i = 0; i < _packs.Count; i++)
        {
            if (!string.Equals(_packs[i].PackId, packId, StringComparison.Ordinal))
            {
                continue;
            }

            _packList.Select(i);
            break;
        }

        RefreshSelection();
    }
}
