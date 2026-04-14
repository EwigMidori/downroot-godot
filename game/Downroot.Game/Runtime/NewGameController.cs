using Godot;

namespace Downroot.Game.Runtime;

public sealed class NewGameController
{
    public event Action<string, string>? CreateRequested;
    public event Action? BackRequested;

    private readonly Control _root;
    private readonly LineEdit _saveName;
    private readonly LineEdit _seed;

    public NewGameController()
    {
        _root = CreatePageRoot("New Game");
        var stack = (VBoxContainer)_root.GetChild(1);

        stack.AddChild(new Label { Text = "Save Name" });
        _saveName = new LineEdit { PlaceholderText = "My World" };
        stack.AddChild(_saveName);

        stack.AddChild(new Label { Text = "World Seed" });
        _seed = new LineEdit { PlaceholderText = "Leave blank to auto-generate" };
        stack.AddChild(_seed);

        var buttons = new HBoxContainer();
        buttons.AddThemeConstantOverride("separation", 10);
        stack.AddChild(buttons);
        var create = new Button { Text = "Create", FocusMode = Control.FocusModeEnum.None };
        create.Pressed += () => CreateRequested?.Invoke(_saveName.Text, _seed.Text);
        var back = new Button { Text = "Back", FocusMode = Control.FocusModeEnum.None };
        back.Pressed += () => BackRequested?.Invoke();
        buttons.AddChild(create);
        buttons.AddChild(back);
    }

    public Control View => _root;

    private static Control CreatePageRoot(string title)
    {
        var root = new Control { AnchorRight = 1, AnchorBottom = 1 };
        root.AddChild(new ColorRect { Color = new Color(0.07f, 0.09f, 0.12f), AnchorRight = 1, AnchorBottom = 1 });
        var stack = new VBoxContainer
        {
            AnchorLeft = 0.5f,
            AnchorTop = 0.5f,
            AnchorRight = 0.5f,
            AnchorBottom = 0.5f,
            OffsetLeft = -180,
            OffsetTop = -120,
            OffsetRight = 180,
            OffsetBottom = 120
        };
        stack.AddThemeConstantOverride("separation", 10);
        stack.AddChild(new Label { Text = title, HorizontalAlignment = HorizontalAlignment.Center });
        root.AddChild(stack);
        return root;
    }
}
