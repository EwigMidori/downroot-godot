using Downroot.Core.Ids;
using Downroot.Core.Input;
using Downroot.Core.Diagnostics;
using Downroot.Game.Infrastructure;
using Downroot.Gameplay.Bootstrap;
using Downroot.Gameplay.Runtime;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Downroot.Game.Runtime;

public partial class GameRoot : Node2D
{
    private GameRuntime? _runtime;
    private GameSimulation? _simulation;
    private IInputService? _inputService;
    private TextureContentLoader? _textureLoader;
    private PlayerAnimationFactory? _animationFactory;
    private StartupOverlayController? _startupOverlay;
    private WorldRenderer? _worldRenderer;
    private HudController? _hudController;
    private CanvasLayer? _travelOverlayLayer;
    private ColorRect? _travelOverlay;

    public override void _Ready()
    {
        try
        {
            _startupOverlay = new StartupOverlayController(this);
            _startupOverlay.Show("Configuring input");
            GameInputMapInstaller.Install();

            _startupOverlay.UpdateStatus("Bootstrapping runtime");
            RuntimeProfiler.Configure(message => GD.Print(message), frameWindow: 60);
            _runtime = new GameBootstrapper().Bootstrap();
            _simulation = new GameSimulation(_runtime);

            _startupOverlay.UpdateStatus("Resolving content root");
            var packPathResolver = new PackPathResolver();
            _textureLoader = new TextureContentLoader(packPathResolver);
            _animationFactory = new PlayerAnimationFactory(packPathResolver);
            GD.Print("Content root resolved.");

            _startupOverlay.UpdateStatus("Creating HUD");
            _hudController = new HudController(this, _textureLoader);
            _hudController.Initialize(_simulation);

            _inputService = new GodotInputService(() =>
            {
                var pointer = GetGlobalMousePosition();
                return new NumericsVector2(pointer.X, pointer.Y);
            }, () => _hudController.IsPointerOverBlockingUi(GetViewport().GetMousePosition()));

            _startupOverlay.UpdateStatus("Creating world renderer");
            _worldRenderer = new WorldRenderer(_textureLoader, _animationFactory);
            AddChild(_worldRenderer);
            _worldRenderer.Initialize(_runtime);
            InitializeTravelOverlay();

            _startupOverlay.UpdateStatus("Validating content");
            _worldRenderer.ValidateContentLoads(_runtime);

            _worldRenderer.Update(new InputFrame(default, default, false, false, false, false, false, false, 0, null));
            _hudController.Refresh(_runtime, _worldRenderer.WorldToScreen);
            _startupOverlay.Hide();
        }
        catch (Exception exception)
        {
            GD.PushError(exception.ToString());
            _startupOverlay ??= new StartupOverlayController(this);
            _startupOverlay.ShowError(exception);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_runtime is null || _simulation is null || _inputService is null || _worldRenderer is null || _hudController is null)
        {
            return;
        }

        RuntimeProfiler.BeginFrame();
        using var frameScope = RuntimeProfiler.Measure("GameRoot.PhysicsProcess");
        var frame = _inputService.CaptureFrame();
        using (RuntimeProfiler.Measure("GameRoot.Simulation"))
        {
            _simulation.Tick((float)delta, frame);
        }

        using (RuntimeProfiler.Measure("GameRoot.Renderer"))
        {
            _worldRenderer.Update(frame);
        }

        using (RuntimeProfiler.Measure("GameRoot.Hud"))
        {
            _hudController.Refresh(_runtime, _worldRenderer.WorldToScreen);
        }

        using (RuntimeProfiler.Measure("GameRoot.Overlay"))
        {
            UpdateTravelOverlay();
        }

        RuntimeProfiler.EndFrame();
    }

    private void InitializeTravelOverlay()
    {
        _travelOverlayLayer = new CanvasLayer();
        _travelOverlay = new ColorRect
        {
            Color = new Color(0f, 0f, 0f, 0f),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        _travelOverlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _travelOverlay.SetOffsetsPreset(Control.LayoutPreset.FullRect);
        _travelOverlayLayer.AddChild(_travelOverlay);
        AddChild(_travelOverlayLayer);
    }

    private void UpdateTravelOverlay()
    {
        if (_runtime is null || _travelOverlay is null)
        {
            return;
        }

        var alpha = _runtime.WorldState.Travel.OverlayAlpha01;
        _travelOverlay.Color = new Color(0f, 0f, 0f, alpha);
        _travelOverlay.Visible = alpha > 0f;
    }
}
