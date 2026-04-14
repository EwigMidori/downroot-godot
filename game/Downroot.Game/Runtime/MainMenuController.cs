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
    private readonly PanelContainer _menuPanel;
    private readonly Label _headingLabel;
    private readonly Label _subheadingLabel;
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

        _menuPanel = new PanelContainer
        {
            AnchorLeft = 1,
            AnchorTop = 0.5f,
            AnchorRight = 1,
            AnchorBottom = 0.5f,
            OffsetLeft = -430,
            OffsetTop = -215,
            OffsetRight = -80,
            OffsetBottom = 215
        };
        _menuPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle(
            new Color(0.05f, 0.08f, 0.09f, 0.44f),
            new Color(0.79f, 0.88f, 0.80f, 0.16f)));
        _root.AddChild(_menuPanel);

        var content = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        content.AddThemeConstantOverride("separation", 10);
        _menuPanel.AddChild(content);

        var header = new VBoxContainer();
        header.AddThemeConstantOverride("separation", 6);
        content.AddChild(header);

        _headingLabel = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _headingLabel.AddThemeFontSizeOverride("font_size", 34);
        _headingLabel.AddThemeColorOverride("font_color", new Color(0.95f, 0.95f, 0.9f));
        header.AddChild(_headingLabel);

        _subheadingLabel = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _subheadingLabel.AddThemeFontSizeOverride("font_size", 15);
        _subheadingLabel.AddThemeColorOverride("font_color", new Color(0.82f, 0.88f, 0.84f, 0.82f));
        header.AddChild(_subheadingLabel);

        var spacer = new Control { CustomMinimumSize = new Vector2(0, 18) };
        content.AddChild(spacer);

        var buttons = new VBoxContainer();
        buttons.AddThemeConstantOverride("separation", 10);
        content.AddChild(buttons);

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
            OffsetLeft = -220,
            OffsetTop = -42,
            OffsetRight = -20,
            OffsetBottom = -18,
            HorizontalAlignment = HorizontalAlignment.Right,
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
        _backdrop.Color = new Color(0.03f, 0.05f, 0.06f, 0.24f);
        _menuPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle(
            new Color(0.05f, 0.08f, 0.09f, 0.44f),
            new Color(0.79f, 0.88f, 0.80f, 0.16f)));
        _headingLabel.Text = data.Heading;
        _subheadingLabel.Text = data.Subheading;
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
        _backdrop.Color = new Color(0.01f, 0.03f, 0.04f, 0.68f);
        _menuPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle(
            new Color(0.04f, 0.07f, 0.08f, 0.86f),
            new Color(0.98f, 0.83f, 0.42f, 0.34f)));
        _headingLabel.Text = "Paused";
        _subheadingLabel.Text = "The session is paused. Resume, save, or return to the main menu.";
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
            CustomMinimumSize = new Vector2(0, 54),
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
        button.AddThemeConstantOverride("h_separation", 14);

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
            BorderWidthLeft = 4,
            ContentMarginLeft = 18,
            ContentMarginRight = 12,
            ContentMarginTop = 10,
            ContentMarginBottom = 10,
            CornerRadiusTopLeft = 3,
            CornerRadiusTopRight = 3,
            CornerRadiusBottomLeft = 3,
            CornerRadiusBottomRight = 3
        };
    }

    private static StyleBoxFlat CreatePanelStyle(Color background, Color border)
    {
        return new StyleBoxFlat
        {
            BgColor = background,
            BorderColor = border,
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            ContentMarginLeft = 26,
            ContentMarginRight = 26,
            ContentMarginTop = 28,
            ContentMarginBottom = 24,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8
        };
    }
}
