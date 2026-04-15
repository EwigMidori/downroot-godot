using Downroot.Core.Definitions;
using Downroot.Core.Ids;
using Downroot.Gameplay.Runtime;
using Godot;
using NumericsVector3 = System.Numerics.Vector3;

namespace Downroot.Game.Runtime;

public sealed partial class WorldLightController : Node2D
{
    private readonly Dictionary<EntityId, PointLight2D> _lights = [];
    private readonly CanvasModulate _canvasModulate = new();
    private Texture2D? _lightTexture;
    private GameRuntime? _runtime;
    private long _lastLightingFieldVersion = -1;

    public WorldLightController()
    {
        Name = "LightingPresentationController";
        ProcessMode = ProcessModeEnum.Pausable;
        _canvasModulate.Color = Colors.White;
        AddChild(_canvasModulate);
    }

    public void Initialize(GameRuntime runtime)
    {
        _runtime = runtime;
        _lightTexture = CreateLightTexture();
        SynchronizeLights();
        UpdateLighting();
    }

    public void UpdateLighting()
    {
        if (_runtime is null)
        {
            return;
        }

        if (_lastLightingFieldVersion != _runtime.WorldState.Lighting.FieldVersion)
        {
            SynchronizeLights();
            _lastLightingFieldVersion = _runtime.WorldState.Lighting.FieldVersion;
        }

        var field = _runtime.WorldState.Lighting.Field;
        var outdoorSkylight = field?.OutdoorSkylightLevel ?? 1f;
        _canvasModulate.Color = ResolveAtmosphereColor(outdoorSkylight);

        foreach (var emitter in _runtime.WorldState.Lighting.Emitters)
        {
            if (!_lights.TryGetValue(emitter.EntityId, out var light))
            {
                continue;
            }

            ApplyEmitterVisual(light, emitter, _runtime.WorldState.TotalElapsedSeconds);
        }
    }

    private void SynchronizeLights()
    {
        var desired = _runtime!.WorldState.Lighting.Emitters
            .Select(emitter => emitter.EntityId)
            .ToHashSet();

        foreach (var stale in _lights.Keys.Where(id => !desired.Contains(id)).ToArray())
        {
            _lights[stale].QueueFree();
            _lights.Remove(stale);
        }

        foreach (var emitter in _runtime.WorldState.Lighting.Emitters)
        {
            if (!_lights.TryGetValue(emitter.EntityId, out var light))
            {
                light = new PointLight2D
                {
                    Texture = _lightTexture,
                    BlendMode = Light2D.BlendModeEnum.Add,
                    ShadowEnabled = false
                };
                AddChild(light);
                _lights[emitter.EntityId] = light;
            }

            ConfigureLight(light, emitter);
            ApplyEmitterVisual(light, emitter, _runtime.WorldState.TotalElapsedSeconds);
        }
    }

    private static void ConfigureLight(PointLight2D light, RuntimeLightEmitter emitter)
    {
        NumericsVector3 color = emitter.Color;
        light.Color = new Color(color.X, color.Y, color.Z, 1f);
        light.TextureScale = emitter.RadiusTiles * 1.18f;
        light.Energy = 1.05f + (emitter.Intensity * 0.55f);
    }

    private static void ApplyEmitterVisual(PointLight2D light, RuntimeLightEmitter emitter, float totalElapsedSeconds)
    {
        var (radiusScale, energyScale) = ResolveFlicker(emitter, totalElapsedSeconds);
        light.Enabled = emitter.IsEnabled;
        light.Visible = emitter.IsEnabled;
        light.Position = new Vector2((emitter.WorldTile.X * 32f) + 16f, (emitter.WorldTile.Y * 32f) + 16f);
        light.TextureScale = emitter.RadiusTiles * 1.18f * radiusScale;
        light.Energy = (1.05f + (emitter.Intensity * 0.55f)) * energyScale;
    }

    private static (float RadiusScale, float EnergyScale) ResolveFlicker(RuntimeLightEmitter emitter, float totalElapsedSeconds)
    {
        return emitter.PresentationKind switch
        {
            LightPresentationKind.Torch => (
                1f + (MathF.Sin(totalElapsedSeconds * 5.2f) * 0.06f),
                1f + (MathF.Sin((totalElapsedSeconds * 4.1f) + 1.7f) * 0.10f)),
            LightPresentationKind.Portal => (
                1f + (MathF.Sin((totalElapsedSeconds * 1.6f) + 0.8f) * 0.03f),
                1f + (MathF.Sin(totalElapsedSeconds * 1.2f) * 0.04f)),
            _ => (1f, 1f)
        };
    }

    private static Color ResolveAtmosphereColor(float outdoorSkylight)
    {
        var t = Mathf.Clamp(outdoorSkylight, 0f, 1f);
        return new Color(
            Mathf.Lerp(0.78f, 1f, t),
            Mathf.Lerp(0.82f, 1f, t),
            Mathf.Lerp(0.92f, 1f, t),
            1f);
    }

    private static Texture2D CreateLightTexture()
    {
        const int size = 256;
        var image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        var center = new Vector2(size * 0.5f, size * 0.5f);
        var maxRadius = size * 0.5f;
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var distance = center.DistanceTo(new Vector2(x, y));
                var t = Mathf.Clamp(1f - (distance / maxRadius), 0f, 1f);
                var alpha = t * t;
                image.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        return ImageTexture.CreateFromImage(image);
    }
}
