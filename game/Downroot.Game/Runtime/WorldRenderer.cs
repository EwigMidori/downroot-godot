using Downroot.Core.Definitions;
using Downroot.Core.Ids;
using Downroot.Core.Input;
using Downroot.Game.Infrastructure;
using Downroot.Gameplay.Runtime;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Downroot.Game.Runtime;

public sealed partial class WorldRenderer : Node2D
{
    private const int TileSize = 32;

    private readonly TextureContentLoader _textureLoader;
    private readonly PlayerAnimationFactory _animationFactory;
    private readonly Dictionary<string, Texture2D> _textureCache = [];
    private readonly Dictionary<EntityId, Sprite2D> _entitySprites = [];

    private GameRuntime? _runtime;
    private Node2D? _terrainLayer;
    private Node2D? _entityLayer;
    private CharacterBody2D? _playerBody;
    private AnimatedSprite2D? _playerSprite;
    private string _lastFacing = "down";

    public WorldRenderer(TextureContentLoader textureLoader, PlayerAnimationFactory animationFactory)
    {
        _textureLoader = textureLoader;
        _animationFactory = animationFactory;
        Name = "WorldRenderer";
    }

    public void Initialize(GameRuntime runtime)
    {
        _runtime = runtime;
        _terrainLayer = new Node2D { Name = "TerrainLayer" };
        _entityLayer = new Node2D { Name = "EntityLayer" };
        AddChild(_terrainLayer);
        AddChild(_entityLayer);

        BuildStaticTerrain();
        CreatePlayer();
        SynchronizeEntities();
    }

    public void Update(InputFrame frame)
    {
        if (_runtime is null || _playerBody is null || _playerSprite is null)
        {
            return;
        }

        _playerBody.Position = ToGodot(_runtime.Player.Position);
        _playerBody.Velocity = ToGodot(frame.Movement * _runtime.Player.Speed);

        if (frame.Movement != NumericsVector2.Zero)
        {
            _lastFacing = ResolveFacing(frame.Movement);
            _playerSprite.Play($"run_{_lastFacing}");
        }
        else
        {
            _playerSprite.Play($"idle_{_lastFacing}");
        }

        SynchronizeEntities();
    }

    public void ValidateContentLoads(GameRuntime runtime)
    {
        _ = ResolveTerrainTexture(runtime.Content.Terrains.Get(new ContentId("basegame:grass")));
        _ = ResolveTerrainTexture(runtime.Content.Terrains.Get(new ContentId("basegame:dirt")));
        _ = ResolveItemTexture(runtime.Content.Items.Get(new ContentId("basegame:stone")));
        _ = ResolveItemTexture(runtime.Content.Items.Get(new ContentId("basegame:voidite")));
        _ = ResolveItemTexture(runtime.Content.Items.Get(new ContentId("basegame:goldvein")));
        _ = ResolveItemTexture(runtime.Content.Items.Get(new ContentId("basegame:venomite")));
        _ = ResolveItemTexture(runtime.Content.Items.Get(new ContentId("basegame:furnace_item")));
        _ = ResolveItemTexture(runtime.Content.Items.Get(new ContentId("basegame:axe")));
        _ = ResolveItemTexture(runtime.Content.Items.Get(new ContentId("basegame:iron_knife")));
        _ = ResolvePlaceableTexture(runtime.Content.Placeables.Get(new ContentId("basegame:workbench")), false);
        _ = ResolvePlaceableTexture(runtime.Content.Placeables.Get(new ContentId("basegame:furnace")), false);
        _ = ResolvePlaceableTexture(runtime.Content.Placeables.Get(new ContentId("basegame:stone_wall")), false);
        _ = ResolvePlaceableTexture(runtime.Content.Placeables.Get(new ContentId("basegame:stone_floor")), false);
        _ = ResolveResourceNodeTexture(runtime.Content.ResourceNodes.Get(new ContentId("basegame:voidite_node")));
        _ = ResolveResourceNodeTexture(runtime.Content.ResourceNodes.Get(new ContentId("basegame:goldvein_node")));
        _ = ResolveResourceNodeTexture(runtime.Content.ResourceNodes.Get(new ContentId("basegame:venomite_node")));
        _ = ResolveCreatureTexture(runtime.Content.Creatures.Get(new ContentId("basegame:cockroach")));
        GD.Print($"Terrain tiles: {runtime.World.Surface.Width}x{runtime.World.Surface.Height}, entities: {runtime.WorldState.Entities.Count}");
    }

    public Vector2 WorldToScreen(NumericsVector2 worldPosition)
    {
        return GetViewport().GetCanvasTransform() * ToGodot(worldPosition);
    }

    public Texture2D ResolveItemIcon(ContentId itemId)
    {
        return ResolveItemTexture(_runtime!.Content.Items.Get(itemId));
    }

    private void BuildStaticTerrain()
    {
        var runtime = _runtime ?? throw new InvalidOperationException("WorldRenderer.Initialize must be called before terrain build.");

        for (var y = 0; y < runtime.World.Surface.Height; y++)
        {
            for (var x = 0; x < runtime.World.Surface.Width; x++)
            {
                var terrainId = runtime.World.Surface.GetTerrainId(x, y) ?? runtime.BootstrapConfig.DefaultTerrainId;
                var terrainDef = runtime.Content.Terrains.Get(terrainId);
                _terrainLayer!.AddChild(new Sprite2D
                {
                    Name = $"Terrain_{x}_{y}",
                    Centered = false,
                    Texture = ResolveTerrainTexture(terrainDef),
                    Position = new Vector2(x * TileSize, y * TileSize)
                });
            }
        }
    }

    private void CreatePlayer()
    {
        var creature = _runtime!.Content.Creatures.Get(_runtime.BootstrapConfig.PlayerCreatureId);
        var frames = _animationFactory.Create(creature);

        _playerBody = new CharacterBody2D
        {
            Name = "Player",
            Position = ToGodot(_runtime.Player.Position)
        };

        _playerSprite = new AnimatedSprite2D
        {
            SpriteFrames = frames,
            Animation = "idle_down"
        };
        _playerSprite.Play("idle_down");
        _playerBody.AddChild(_playerSprite);

        var camera = new Camera2D
        {
            Enabled = true,
            PositionSmoothingEnabled = true,
            PositionSmoothingSpeed = 6f
        };
        _playerBody.AddChild(camera);
        AddChild(_playerBody);
    }

    private void SynchronizeEntities()
    {
        var aliveIds = _runtime!.WorldState.Entities.Where(entity => !entity.Removed).Select(entity => entity.Id).ToHashSet();

        foreach (var removedId in _entitySprites.Keys.Where(id => !aliveIds.Contains(id)).ToArray())
        {
            _entitySprites[removedId].QueueFree();
            _entitySprites.Remove(removedId);
        }

        foreach (var entity in _runtime.WorldState.Entities.Where(entity => !entity.Removed))
        {
            if (!_entitySprites.TryGetValue(entity.Id, out var sprite))
            {
                sprite = new Sprite2D { Centered = false };
                _entityLayer!.AddChild(sprite);
                _entitySprites.Add(entity.Id, sprite);
            }

            sprite.Texture = entity.Kind switch
            {
                WorldEntityKind.ResourceNode => ResolveResourceNodeTexture(_runtime.Content.ResourceNodes.Get(entity.DefinitionId)),
                WorldEntityKind.Placeable => ResolvePlaceableTexture(_runtime.Content.Placeables.Get(entity.DefinitionId), entity.OpenState),
                WorldEntityKind.Creature => ResolveCreatureTexture(_runtime.Content.Creatures.Get(entity.DefinitionId)),
                WorldEntityKind.ItemDrop => ResolveItemTexture(_runtime.Content.Items.Get(entity.DefinitionId)),
                _ => throw new InvalidOperationException($"Unsupported entity kind '{entity.Kind}'.")
            };
            sprite.Position = ToGodot(entity.Position);
            sprite.FlipH = entity.Kind == WorldEntityKind.Creature && entity.Position.X > _runtime.Player.Position.X;
        }
    }

    private Texture2D ResolveTerrainTexture(TerrainDef terrainDef) => ResolveCachedTexture($"terrain:{terrainDef.Id.Value}", () => _textureLoader.LoadTerrain(terrainDef).Texture);

    private Texture2D ResolveItemTexture(ItemDef itemDef) => ResolveCachedTexture($"item:{itemDef.Id.Value}", () => _textureLoader.LoadItem(itemDef).Texture);

    private Texture2D ResolvePlaceableTexture(PlaceableDef placeableDef, bool isOpen)
    {
        return ResolveCachedTexture($"placeable:{placeableDef.Id.Value}:{isOpen}", () => _textureLoader.LoadPlaceable(placeableDef, isOpen).Texture);
    }

    private Texture2D ResolveResourceNodeTexture(ResourceNodeDef resourceNodeDef)
    {
        return ResolveCachedTexture($"resource:{resourceNodeDef.Id.Value}", () => _textureLoader.LoadResourceNode(resourceNodeDef).Texture);
    }

    private Texture2D ResolveCreatureTexture(CreatureDef creatureDef)
    {
        return ResolveCachedTexture($"creature:{creatureDef.Id.Value}", () => _textureLoader.LoadCreature(creatureDef).Texture);
    }

    private Texture2D ResolveCachedTexture(string key, Func<Texture2D> factory)
    {
        if (_textureCache.TryGetValue(key, out var texture))
        {
            return texture;
        }

        texture = factory();
        _textureCache[key] = texture;
        return texture;
    }

    private static string ResolveFacing(NumericsVector2 movement)
    {
        if (MathF.Abs(movement.X) > MathF.Abs(movement.Y))
        {
            return movement.X > 0 ? "right" : "left";
        }

        return movement.Y > 0 ? "down" : "up";
    }

    private static Vector2 ToGodot(NumericsVector2 vector) => new(vector.X, vector.Y);
}
