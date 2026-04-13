using Godot;

namespace Downroot.Game.Runtime;

public sealed class StartupOverlayController
{
    private readonly Node _host;
    private CanvasLayer? _overlay;
    private Label? _label;

    public StartupOverlayController(Node host)
    {
        _host = host;
    }

    public void Show(string status)
    {
        EnsureOverlay();
        UpdateStatus(status);
    }

    public void UpdateStatus(string status)
    {
        GD.Print($"[Boot] {status}");
        if (_label is not null)
        {
            _label.Text = $"Booting Downroot...\n{status}";
        }
    }

    public void ShowError(Exception exception)
    {
        EnsureOverlay();
        if (_label is not null)
        {
            _label.Text = $"Startup failed:\n{exception}";
        }
    }

    public void Hide()
    {
        _overlay?.QueueFree();
        _overlay = null;
        _label = null;
    }

    private void EnsureOverlay()
    {
        if (_overlay is not null)
        {
            return;
        }

        _overlay = new CanvasLayer();
        _host.AddChild(_overlay);

        _overlay.AddChild(new ColorRect
        {
            Color = new Color(0.04f, 0.05f, 0.07f, 0.92f),
            AnchorRight = 1,
            AnchorBottom = 1
        });

        _label = new Label
        {
            Position = new Vector2(16, 16)
        };
        _overlay.AddChild(_label);
    }
}
