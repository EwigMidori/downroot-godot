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
    private readonly TextureRect _background;
    private readonly ColorRect _backdrop;
    private readonly VBoxContainer _menuColumn;
    private readonly Label _pauseLabel;
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
            AnchorBottom = 1,
            ProcessMode = Node.ProcessModeEnum.Always
        };

        _background = new TextureRect
        {
            AnchorRight = 1,
            AnchorBottom = 1,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
            Texture = GD.Load<Texture2D>("res://assets/ui/main_menu_background.png")
        };
        _root.AddChild(_background);

        _backdrop = new ColorRect
        {
            AnchorRight = 1,
            AnchorBottom = 1,
            Color = new Color(0.03f, 0.05f, 0.06f, 0.24f)
        };
        _root.AddChild(_backdrop);

        _menuColumn = new VBoxContainer
        {
            AnchorLeft = 0,
            AnchorTop = 1,
            AnchorRight = 0,
            AnchorBottom = 1,
            OffsetLeft = 54,
            OffsetTop = -380,
            OffsetRight = 394,
            OffsetBottom = -54
        };
        _menuColumn.AddThemeConstantOverride("separation", 8);
        _root.AddChild(_menuColumn);

        _pauseLabel = new Label
        {
            Text = "Paused",
            Visible = false,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        _pauseLabel.AddThemeFontSizeOverride("font_size", 18);
        _pauseLabel.AddThemeColorOverride("font_color", new Color(0.98f, 0.85f, 0.48f, 0.95f));
        _menuColumn.AddChild(_pauseLabel);

        _continueButton = CreateButton("Continue", () => ContinueRequested?.Invoke());
        _newGameButton = CreateButton("New Game", () => NewGameRequested?.Invoke());
        _quickStartButton = CreateButton("Quick Start", () => QuickStartRequested?.Invoke());
        _loadGameButton = CreateButton("Load Game", () => LoadGameRequested?.Invoke());
        _settingsButton = CreateButton("Settings", () => SettingsRequested?.Invoke());
        _quitButton = CreateButton("Quit", () => QuitRequested?.Invoke());

        _menuColumn.AddChild(_continueButton);
        _menuColumn.AddChild(_newGameButton);
        _menuColumn.AddChild(_quickStartButton);
        _menuColumn.AddChild(_loadGameButton);
        _menuColumn.AddChild(_settingsButton);
        _menuColumn.AddChild(_quitButton);

        _versionLabel = new Label
        {
            AnchorLeft = 0,
            AnchorTop = 1,
            AnchorRight = 0,
            AnchorBottom = 1,
            OffsetLeft = 54,
            OffsetTop = -42,
            OffsetRight = 394,
            OffsetBottom = -18,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom
        };
        _versionLabel.AddThemeFontSizeOverride("font_size", 13);
        _versionLabel.AddThemeColorOverride("font_color", new Color(0.94f, 0.92f, 0.82f, 0.76f));
        _root.AddChild(_versionLabel);
    }

    public Control View => _root;

    public void Bind(MainMenuViewData data)
    {
        _background.Visible = true;
        _backdrop.Color = new Color(0.02f, 0.04f, 0.05f, 0.18f);
        _pauseLabel.Visible = false;
        _continueButton.Disabled = !data.CanContinue;
        _loadGameButton.Disabled = !data.CanLoadGame;
        ConfigureButton(_continueButton, "Continue", true);
        ConfigureButton(_newGameButton, "New Game", true);
        ConfigureButton(_quickStartButton, "Quick Start", true);
        ConfigureButton(_loadGameButton, "Load Game", true);
        ConfigureButton(_settingsButton, "Settings", true);
        ConfigureButton(_quitButton, "Quit", true);
        _versionLabel.Visible = true;
        _versionLabel.Text = data.VersionLabel;
    }

    public void BindPauseMenu(bool canSaveGame)
    {
        _background.Visible = false;
        _backdrop.Color = new Color(0.01f, 0.03f, 0.04f, 0.72f);
        _pauseLabel.Visible = true;
        ConfigureButton(_continueButton, "Resume", true);
        ConfigureButton(_newGameButton, "Save Game", true);
        ConfigureButton(_quickStartButton, string.Empty, false);
        ConfigureButton(_loadGameButton, "Return to Main Menu", true);
        ConfigureButton(_settingsButton, "Settings", true);
        ConfigureButton(_quitButton, "Quit Desktop", true);
        _continueButton.Disabled = false;
        _newGameButton.Disabled = !canSaveGame;
        _loadGameButton.Disabled = false;
        _settingsButton.Disabled = false;
        _quitButton.Disabled = false;
        _versionLabel.Visible = false;
    }

    private static Button CreateButton(string text, Action pressed)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(280, 42),
            Alignment = HorizontalAlignment.Left,
            FocusMode = Control.FocusModeEnum.None
        };
        ApplyButtonTheme(button);
        button.Pressed += pressed;
        return button;
    }

    private static void ConfigureButton(Button button, string text, bool visible)
    {
        button.Text = text;
        button.Visible = visible;
    }

    private static void ApplyButtonTheme(Button button)
    {
        button.AddThemeFontSizeOverride("font_size", 22);
        button.AddThemeColorOverride("font_color", new Color(0.91f, 0.93f, 0.9f, 0.9f));
        button.AddThemeColorOverride("font_hover_color", new Color(1f, 0.95f, 0.78f));
        button.AddThemeColorOverride("font_pressed_color", new Color(1f, 0.96f, 0.86f));
        button.AddThemeColorOverride("font_disabled_color", new Color(0.73f, 0.78f, 0.77f, 0.42f));
        button.AddThemeConstantOverride("h_separation", 10);

        button.AddThemeStyleboxOverride("normal", CreateButtonStyle(
            new Color(0f, 0f, 0f, 0f),
            new Color(1f, 1f, 1f, 0f)));
        button.AddThemeStyleboxOverride("hover", CreateButtonStyle(
            new Color(0.92f, 0.8f, 0.37f, 0.12f),
            new Color(0.95f, 0.83f, 0.45f, 0.72f)));
        button.AddThemeStyleboxOverride("pressed", CreateButtonStyle(
            new Color(0.92f, 0.8f, 0.37f, 0.18f),
            new Color(0.98f, 0.9f, 0.58f, 0.92f)));
        button.AddThemeStyleboxOverride("focus", CreateButtonStyle(
            new Color(0.92f, 0.8f, 0.37f, 0.14f),
            new Color(0.95f, 0.83f, 0.45f, 0.86f)));
        button.AddThemeStyleboxOverride("disabled", CreateButtonStyle(
            new Color(0f, 0f, 0f, 0f),
            new Color(1f, 1f, 1f, 0f)));
    }

    private static StyleBoxFlat CreateButtonStyle(Color background, Color border)
    {
        return new StyleBoxFlat
        {
            BgColor = background,
            DrawCenter = true,
            BorderColor = border,
            BorderWidthLeft = 3,
            ContentMarginLeft = 14,
            ContentMarginRight = 10,
            ContentMarginTop = 8,
            ContentMarginBottom = 8,
            CornerRadiusTopLeft = 3,
            CornerRadiusTopRight = 3,
            CornerRadiusBottomLeft = 3,
            CornerRadiusBottomRight = 3
        };
    }
}
