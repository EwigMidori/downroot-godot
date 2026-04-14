using Downroot.UI.Presentation;
using Godot;

namespace Downroot.Game.Runtime;

public sealed class MainMenuController
{
    public event Action? ContinueRequested;
    public event Action? NewGameRequested;
    public event Action? QuickStartRequested;
    public event Action? LoadGameRequested;
    public event Action? SettingsRequested;
    public event Action? QuitRequested;

    private readonly Control _root;
    private readonly Button _continueButton;
    private readonly Button _newGameButton;
    private readonly Button _quickStartButton;
    private readonly Button _loadGameButton;
    private readonly Button _settingsButton;
    private readonly Button _quitButton;
    private readonly Label _versionLabel;

    public MainMenuController()
    {
        _root = new Control
        {
            Name = "MainMenu",
            AnchorRight = 1,
            AnchorBottom = 1
        };

        var background = new ColorRect
        {
            Color = new Color(0.07f, 0.09f, 0.12f),
            AnchorRight = 1,
            AnchorBottom = 1
        };
        _root.AddChild(background);

        var title = new Label
        {
            Text = "Downroot",
            Position = new Vector2(0, 120),
            AnchorLeft = 0.5f,
            AnchorRight = 0.5f,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        title.OffsetLeft = -180;
        title.OffsetRight = 180;
        _root.AddChild(title);

        var buttons = new VBoxContainer
        {
            AnchorLeft = 0.5f,
            AnchorTop = 0.5f,
            AnchorRight = 0.5f,
            AnchorBottom = 0.5f,
            OffsetLeft = -120,
            OffsetTop = -140,
            OffsetRight = 120,
            OffsetBottom = 140
        };
        buttons.AddThemeConstantOverride("separation", 10);
        _root.AddChild(buttons);

        _continueButton = CreateButton("Continue", () => ContinueRequested?.Invoke());
        _newGameButton = CreateButton("New Game", () => NewGameRequested?.Invoke());
        _quickStartButton = CreateButton("Quick Start", () => QuickStartRequested?.Invoke());
        _loadGameButton = CreateButton("Load Game", () => LoadGameRequested?.Invoke());
        _settingsButton = CreateButton("Settings", () => SettingsRequested?.Invoke());
        _quitButton = CreateButton("Quit", () => QuitRequested?.Invoke());

        buttons.AddChild(_continueButton);
        buttons.AddChild(_newGameButton);
        buttons.AddChild(_quickStartButton);
        buttons.AddChild(_loadGameButton);
        buttons.AddChild(_settingsButton);
        buttons.AddChild(_quitButton);

        _versionLabel = new Label
        {
            AnchorLeft = 1,
            AnchorTop = 1,
            AnchorRight = 1,
            AnchorBottom = 1,
            OffsetLeft = -180,
            OffsetTop = -34,
            OffsetRight = -12,
            OffsetBottom = -12,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom
        };
        _root.AddChild(_versionLabel);
    }

    public Control View => _root;

    public void Bind(MainMenuViewData data)
    {
        _continueButton.Disabled = !data.CanContinue;
        _loadGameButton.Disabled = !data.CanLoadGame;
        _versionLabel.Text = data.VersionLabel;
    }

    private static Button CreateButton(string text, Action pressed)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(0, 42),
            FocusMode = Control.FocusModeEnum.None
        };
        button.Pressed += pressed;
        return button;
    }
}
