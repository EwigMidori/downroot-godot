using Downroot.Core.Save;
using Downroot.UI.Presentation;
using Godot;

namespace Downroot.Game.Runtime;

public sealed class SettingsController
{
    public event Action<GameSettingsData>? ApplyRequested;
    public event Action? BackRequested;

    private readonly Control _root;
    private readonly CheckBox _fullscreen;
    private readonly CheckBox _vsync;
    private readonly HSlider _volume;
    private readonly HSlider _uiScale;

    public SettingsController()
    {
        _root = new Control { AnchorRight = 1, AnchorBottom = 1 };
        _root.AddChild(new ColorRect { Color = new Color(0.07f, 0.09f, 0.12f), AnchorRight = 1, AnchorBottom = 1 });
        var stack = new VBoxContainer
        {
            AnchorLeft = 0.5f,
            AnchorTop = 0.5f,
            AnchorRight = 0.5f,
            AnchorBottom = 0.5f,
            OffsetLeft = -220,
            OffsetTop = -150,
            OffsetRight = 220,
            OffsetBottom = 150
        };
        stack.AddThemeConstantOverride("separation", 12);
        _root.AddChild(stack);

        stack.AddChild(new Label { Text = "Settings", HorizontalAlignment = HorizontalAlignment.Center });
        _fullscreen = new CheckBox { Text = "Fullscreen", FocusMode = Control.FocusModeEnum.None };
        _vsync = new CheckBox { Text = "VSync", FocusMode = Control.FocusModeEnum.None };
        _volume = new HSlider { MinValue = 0, MaxValue = 1, Step = 0.01 };
        _uiScale = new HSlider { MinValue = 0.75f, MaxValue = 1.75f, Step = 0.05f };
        stack.AddChild(_fullscreen);
        stack.AddChild(_vsync);
        stack.AddChild(CreateSliderRow("Master Volume", _volume));
        stack.AddChild(CreateSliderRow("UI Scale", _uiScale));

        var buttons = new HBoxContainer();
        buttons.AddThemeConstantOverride("separation", 10);
        var apply = new Button { Text = "Apply", FocusMode = Control.FocusModeEnum.None };
        apply.Pressed += () => ApplyRequested?.Invoke(ReadCurrent());
        var back = new Button { Text = "Back", FocusMode = Control.FocusModeEnum.None };
        back.Pressed += () => BackRequested?.Invoke();
        buttons.AddChild(apply);
        buttons.AddChild(back);
        stack.AddChild(buttons);
    }

    public Control View => _root;

    public void Bind(SettingsViewData data)
    {
        _fullscreen.ButtonPressed = data.Fullscreen;
        _vsync.ButtonPressed = data.VSync;
        _volume.Value = data.MasterVolume;
        _uiScale.Value = data.UiScale;
    }

    private GameSettingsData ReadCurrent()
    {
        return new GameSettingsData
        {
            Fullscreen = _fullscreen.ButtonPressed,
            VSync = _vsync.ButtonPressed,
            MasterVolume = (float)_volume.Value,
            UiScale = (float)_uiScale.Value
        };
    }

    private static Control CreateSliderRow(string label, Godot.Range slider)
    {
        var row = new VBoxContainer();
        row.AddChild(new Label { Text = label });
        row.AddChild((Control)slider);
        return row;
    }
}
