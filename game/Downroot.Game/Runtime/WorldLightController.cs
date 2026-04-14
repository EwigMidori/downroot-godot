using Downroot.Core.Definitions;
using Downroot.Core.Ids;
using Downroot.Core.World;
using Downroot.Gameplay.Runtime;
using Godot;

namespace Downroot.Game.Runtime;

public sealed partial class WorldLightController : Node2D
{
    private readonly Dictionary<EntityId, PointLight2D> _lights = [];
    private readonly CanvasModulate _canvasModulate = new();
    private Texture2D? _lightTexture;
    private GameRuntime? _runtime;
    private WorldRuntimeFacade? _worldFacade;
    private WorldSpaceKind? _lastWorldSpaceKind;
    private long _lastProjectionVersion = -1;
    private long _lastLightStateVersion = -1;

    public WorldLightController()
    {
        Name = "WorldLightController";
        ProcessMode = ProcessModeEnum.Pausable;
        _canvasModulate.Color = Colors.White;
        AddChild(_canvasModulate);
    }

    public void Initialize(GameRuntime runtime)
    {
        _runtime = runtime;
        _worldFacade = new WorldRuntimeFacade(runtime);
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

        var activeWorld = _worldFacade!.GetActiveWorld();
        if (_lastWorldSpaceKind != activeWorld.WorldSpaceKind
            || _lastProjectionVersion != _runtime.WorldState.EntityProjectionVersion
            || _lastLightStateVersion != _runtime.WorldState.LightStateVersion)
        {
            SynchronizeLights();
            _lastWorldSpaceKind = activeWorld.WorldSpaceKind;
            _lastProjectionVersion = _runtime.WorldState.EntityProjectionVersion;
            _lastLightStateVersion = _runtime.WorldState.LightStateVersion;
        }

        var overlayStrength = ResolveNightOverlayStrength();
        var isNight = overlayStrength > 0.01f;
        _canvasModulate.Color = isNight
            ? new Color(0.26f + overlayStrength * 0.18f, 0.28f + overlayStrength * 0.16f, 0.38f + overlayStrength * 0.1f)
            : Colors.White;

        foreach (var pair in _lights)
        {
            if (!_worldFacade.TryGetActiveEntity(pair.Key, out var entity) || entity.Removed)
            {
                continue;
            }

            pair.Value.Enabled = isNight && EmitsLight(entity);
            pair.Value.Visible = pair.Value.Enabled;
            pair.Value.Position = new Vector2(entity.Position.X + 16f, entity.Position.Y + 16f);
        }
    }

    private void SynchronizeLights()
    {
        var desired = _runtime!.WorldState.Entities
            .Where(entity => entity.Kind == WorldEntityKind.Placeable && EmitsPotentialLight(entity))
            .Select(entity => entity.Id)
            .ToHashSet();

        foreach (var stale in _lights.Keys.Where(id => !desired.Contains(id)).ToArray())
        {
            _lights[stale].QueueFree();
            _lights.Remove(stale);
        }

        foreach (var entity in _runtime.WorldState.Entities.Where(entity => entity.Kind == WorldEntityKind.Placeable && EmitsPotentialLight(entity)))
        {
            if (!_lights.TryGetValue(entity.Id, out var light))
            {
                light = new PointLight2D
                {
                    Texture = _lightTexture,
                    BlendMode = Light2D.BlendModeEnum.Add,
                    ShadowEnabled = false
                };
                AddChild(light);
                _lights[entity.Id] = light;
            }

            ConfigureLight(light, entity);
            light.Position = new Vector2(entity.Position.X + 16f, entity.Position.Y + 16f);
        }
    }

    private void ConfigureLight(PointLight2D light, WorldEntityState entity)
    {
        if (_worldFacade!.IsPortalEntity(entity))
        {
            light.Color = new Color(0.52f, 0.88f, 1f, 1f);
            light.TextureScale = 7.5f;
            light.Energy = 1.7f;
            return;
        }

        light.Color = new Color(1f, 0.82f, 0.48f, 1f);
        light.TextureScale = 4.6f;
        light.Energy = 1.35f;
    }

    private bool EmitsPotentialLight(WorldEntityState entity)
    {
        return _worldFacade!.IsPortalEntity(entity)
            || (_worldFacade.TryGetPlaceableDef(entity, out var placeableDef) && placeableDef.HasBehavior(PlaceableBehaviorKind.LightSource));
    }

    private bool EmitsLight(WorldEntityState entity)
    {
        if (_worldFacade!.IsPortalEntity(entity))
        {
            return true;
        }

        return _worldFacade.TryGetPlaceableDef(entity, out var placeableDef)
            && placeableDef.HasBehavior(PlaceableBehaviorKind.LightSource)
            && entity.PlaceableState?.IsLit == true
            && entity.PlaceableState.FuelSecondsRemaining > 0f;
    }

    private float ResolveNightOverlayStrength()
    {
        var dayLength = _runtime!.BootstrapConfig.DayLengthSeconds;
        if (dayLength <= 0f)
        {
            return 0f;
        }

        var timeProgress = _runtime.WorldState.TimeOfDaySeconds / dayLength;
        var cycle = (timeProgress - 0.25f) * Mathf.Pi * 2f;
        var nightAmount = 0.5f - (0.5f * Mathf.Cos(cycle));
        return Mathf.Clamp(nightAmount * 0.34f, 0f, 0.34f);
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
