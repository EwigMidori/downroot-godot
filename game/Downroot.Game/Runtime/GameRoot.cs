using Downroot.Content.Packs;
using Downroot.Core.Ids;
using Downroot.Core.Input;
using Downroot.Game.Infrastructure;
using Downroot.Gameplay.Bootstrap;
using Downroot.Gameplay.Runtime;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Downroot.Game.Runtime;

public partial class GameRoot : Node2D
{
    private const int TileSize = 32;
    private GameRuntime? _runtime;
    private IInputService? _inputService;
    private AnimatedSprite2D? _playerSprite;
    private CharacterBody2D? _playerBody;
    private string _lastFacing = "down";

    public override void _Ready()
    {
        ConfigureInputMap();

        _inputService = new GodotInputService();
        _runtime = new GameBootstrapper().Bootstrap();

        var packPathResolver = new PackPathResolver();
        var textureLoader = new TextureContentLoader(packPathResolver);
        var animationFactory = new PlayerAnimationFactory(packPathResolver);
        var report = new ContentLoadReport();

        var worldLayer = new Node2D { Name = "WorldLayer" };
        AddChild(worldLayer);

        ValidateAndRenderWorld(worldLayer, textureLoader, report);
        CreatePlayer(animationFactory, report);
        CreateDebugOverlay(report);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_runtime is null || _inputService is null || _playerBody is null || _playerSprite is null)
        {
            return;
        }

        var movement = _inputService.GetMovementVector();
        _runtime.Player.Position += movement * _runtime.Player.Speed * (float)delta;
        _playerBody.Position = ToGodot(_runtime.Player.Position);
        _playerBody.Velocity = ToGodot(movement * _runtime.Player.Speed);

        var animationName = ResolveAnimationName(movement);
        if (_playerSprite.Animation != animationName)
        {
            _playerSprite.Play(animationName);
        }
        else if (movement == NumericsVector2.Zero && !_playerSprite.IsPlaying())
        {
            _playerSprite.Play(animationName);
        }
    }

    private void ValidateAndRenderWorld(Node2D worldLayer, TextureContentLoader textureLoader, ContentLoadReport report)
    {
        var grassDef = _runtime!.Content.Terrains.Get(new ContentId("basegame:grass"));
        var dirtDef = _runtime.Content.Terrains.Get(new ContentId("basegame:dirt"));
        var itemDef = _runtime.Content.Items.Get(new ContentId("basegame:stone"));
        var placeableDef = _runtime.Content.Placeables.Get(new ContentId("basegame:wooden_chest"));

        var grassTexture = textureLoader.LoadTerrain(grassDef);
        var dirtTexture = textureLoader.LoadTerrain(dirtDef);
        var itemTexture = textureLoader.LoadItem(itemDef);
        var placeableTexture = textureLoader.LoadPlaceable(placeableDef);

        report.AddSuccess(grassTexture.ContentId, grassTexture.AbsolutePath);
        report.AddSuccess(dirtTexture.ContentId, dirtTexture.AbsolutePath);
        report.AddSuccess(itemTexture.ContentId, itemTexture.AbsolutePath);
        report.AddSuccess(placeableTexture.ContentId, placeableTexture.AbsolutePath);

        for (var y = 0; y < _runtime.World.Surface.Height; y++)
        {
            for (var x = 0; x < _runtime.World.Surface.Width; x++)
            {
                var terrain = _runtime.World.Surface.GetTerrain(x, y)
                    ?? throw new InvalidOperationException($"Missing terrain at {x},{y}.");

                var sprite = new Sprite2D
                {
                    Texture = terrain.Id.Value == dirtDef.Id.Value ? dirtTexture.Texture : grassTexture.Texture,
                    Centered = false,
                    Position = new Godot.Vector2(x * TileSize, y * TileSize)
                };

                worldLayer.AddChild(sprite);
            }
        }

        var chestPreview = new Sprite2D
        {
            Name = "PlaceablePreview",
            Texture = placeableTexture.Texture,
            Centered = false,
            Position = new Godot.Vector2(5 * TileSize, 5 * TileSize)
        };
        worldLayer.AddChild(chestPreview);
    }

    private void CreatePlayer(PlayerAnimationFactory animationFactory, ContentLoadReport report)
    {
        var creature = _runtime!.Content.Creatures.Get(new ContentId("basegame:player_human"));
        var spriteFrames = animationFactory.Create(creature);
        report.AddSuccess(creature.Id.Value, "player animations loaded from idle/run spritesheets");

        _playerBody = new CharacterBody2D
        {
            Name = "Player",
            Position = ToGodot(_runtime.Player.Position)
        };

        _playerSprite = new AnimatedSprite2D
        {
            SpriteFrames = spriteFrames,
            Animation = "idle_down",
            Position = new Godot.Vector2(0, 0)
        };
        _playerSprite.Play("idle_down");

        _playerBody.AddChild(_playerSprite);
        AddChild(_playerBody);
    }

    private void CreateDebugOverlay(ContentLoadReport report)
    {
        var label = new Label
        {
            Name = "DebugLabel",
            Text = $"Phase 0 Runtime Ready\nPacks: {BaseGameContentPack.Id}\nWorldGen: fill-terrain + dirt-patch\n{report.ToDisplayText()}",
            Position = new Godot.Vector2(8, 8)
        };

        var canvasLayer = new CanvasLayer();
        canvasLayer.AddChild(label);
        AddChild(canvasLayer);
    }

    private static void ConfigureInputMap()
    {
        EnsureAction("move_left", Key.A);
        EnsureAction("move_right", Key.D);
        EnsureAction("move_up", Key.W);
        EnsureAction("move_down", Key.S);
    }

    private static void EnsureAction(string actionName, Key key)
    {
        if (!InputMap.HasAction(actionName))
        {
            InputMap.AddAction(actionName);
        }

        foreach (var existing in InputMap.ActionGetEvents(actionName))
        {
            if (existing is InputEventKey keyEvent && keyEvent.PhysicalKeycode == key)
            {
                return;
            }
        }

        InputMap.ActionAddEvent(actionName, new InputEventKey
        {
            PhysicalKeycode = key
        });
    }

    private string ResolveAnimationName(NumericsVector2 movement)
    {
        if (movement == NumericsVector2.Zero)
        {
            return $"idle_{_lastFacing}";
        }

        if (MathF.Abs(movement.X) > MathF.Abs(movement.Y))
        {
            _lastFacing = movement.X > 0 ? "right" : "left";
            return $"run_{_lastFacing}";
        }

        _lastFacing = movement.Y > 0 ? "down" : "up";
        return $"run_{_lastFacing}";
    }

    private static Godot.Vector2 ToGodot(NumericsVector2 vector) => new(vector.X, vector.Y);
}
