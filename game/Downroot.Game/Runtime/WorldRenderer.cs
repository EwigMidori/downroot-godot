using Downroot.Core.Definitions;
using Downroot.Core.Ids;
using Downroot.Core.Input;
using Downroot.Core.World;
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
    private readonly Dictionary<EntityId, ChunkCoord> _entitySpriteChunks = [];
    private readonly Dictionary<ChunkCoord, ChunkVisualState> _chunkVisuals = [];

    private GameRuntime? _runtime;
    private WorldRuntimeFacade? _worldFacade;
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
        _worldFacade = new WorldRuntimeFacade(runtime);
        _terrainLayer = new Node2D { Name = "TerrainLayer" };
        _entityLayer = new Node2D { Name = "EntityLayer" };
        AddChild(_terrainLayer);
        AddChild(_entityLayer);
        CreatePlayer();
        SynchronizeChunks();
        RefreshDirtyRaisedFeatures();
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

        SynchronizeChunks();
        SynchronizeEntities();
    }

    public void ValidateContentLoads(GameRuntime runtime)
    {
        foreach (var terrain in runtime.Content.Terrains.All.Take(4))
        {
            _ = ResolveTerrainTexture(terrain);
        }

        foreach (var item in runtime.Content.Items.All.Take(4))
        {
            _ = ResolveItemTexture(item);
        }

        foreach (var raisedFeature in runtime.Content.RaisedFeatures.All.Take(2))
        {
            _ = ResolveRaisedFeatureTexture(raisedFeature, 0);
        }

        foreach (var placeable in runtime.Content.Placeables.All.Take(2))
        {
            _ = ResolvePlaceableTexture(placeable, false);
        }

        foreach (var resourceNode in runtime.Content.ResourceNodes.All.Take(2))
        {
            _ = ResolveResourceNodeTexture(resourceNode);
        }

        GD.Print($"Loaded chunks: {runtime.WorldState.GetActiveWorld().LoadedChunks.Count}, active entities: {runtime.WorldState.Entities.Count}");
    }

    public Vector2 WorldToScreen(NumericsVector2 worldPosition)
    {
        return GetViewport().GetCanvasTransform() * ToGodot(worldPosition);
    }

    public Texture2D ResolveItemIcon(ContentId itemId)
    {
        return ResolveItemTexture(_runtime!.Content.Items.Get(itemId));
    }

    private void SynchronizeChunks()
    {
        var world = _worldFacade!.GetActiveWorld();
        var desiredChunks = world.LoadedChunks.Keys.ToHashSet();
        foreach (var staleChunk in _chunkVisuals.Keys.Where(coord => !desiredChunks.Contains(coord)).ToArray())
        {
            _chunkVisuals[staleChunk].TerrainRoot.QueueFree();
            _chunkVisuals[staleChunk].RaisedFeatureRoot.QueueFree();
            _chunkVisuals[staleChunk].EntityRoot.QueueFree();
            _chunkVisuals.Remove(staleChunk);
        }

        foreach (var pair in world.LoadedChunks.OrderBy(pair => pair.Key.Y).ThenBy(pair => pair.Key.X))
        {
            if (_chunkVisuals.ContainsKey(pair.Key))
            {
                continue;
            }

            var terrainRoot = new Node2D { Name = $"ChunkTerrain_{pair.Key.X}_{pair.Key.Y}" };
            var raisedFeatureRoot = new Node2D { Name = $"ChunkRaised_{pair.Key.X}_{pair.Key.Y}" };
            var entityRoot = new Node2D { Name = $"ChunkEntities_{pair.Key.X}_{pair.Key.Y}" };
            _terrainLayer!.AddChild(terrainRoot);
            _terrainLayer.AddChild(raisedFeatureRoot);
            _entityLayer!.AddChild(entityRoot);
            _chunkVisuals.Add(pair.Key, new ChunkVisualState(terrainRoot, raisedFeatureRoot, entityRoot));
            BuildChunkTerrain(pair.Value.GeneratedChunk, terrainRoot);
            BuildChunkRaisedFeatures(pair.Value, _chunkVisuals[pair.Key]);
        }
    }

    private void BuildChunkTerrain(Downroot.World.Models.GeneratedChunk chunk, Node2D terrainRoot)
    {
        var chunkOriginTile = WorldTileCoord.FromChunkAndLocal(chunk.Coord, new LocalTileCoord(0, 0), _runtime!.ChunkWidth, _runtime.ChunkHeight);
        for (var y = 0; y < chunk.Surface.Height; y++)
        {
            for (var x = 0; x < chunk.Surface.Width; x++)
            {
                var terrainId = chunk.Surface.GetTerrainId(x, y) ?? _runtime.BootstrapConfig.DefaultTerrainId;
                var terrainDef = _runtime.Content.Terrains.Get(terrainId);
                terrainRoot.AddChild(new Sprite2D
                {
                    Name = $"Terrain_{x}_{y}",
                    Centered = false,
                    Texture = ResolveTerrainTexture(terrainDef),
                    Position = new Vector2((chunkOriginTile.X + x) * TileSize, (chunkOriginTile.Y + y) * TileSize)
                });
            }
        }
    }

    private void BuildChunkRaisedFeatures(ChunkRuntimeState chunk, ChunkVisualState visual)
    {
        var chunkOriginTile = WorldTileCoord.FromChunkAndLocal(chunk.GeneratedChunk.Coord, new LocalTileCoord(0, 0), _runtime!.ChunkWidth, _runtime.ChunkHeight);
        for (var y = 0; y < chunk.GeneratedChunk.Surface.Height; y++)
        {
            for (var x = 0; x < chunk.GeneratedChunk.Surface.Width; x++)
            {
                RefreshRaisedFeatureTile(new WorldTileCoord(chunkOriginTile.X + x, chunkOriginTile.Y + y), visual);
            }
        }
    }

    private void RefreshDirtyRaisedFeatures()
    {
        var world = _worldFacade!.GetActiveWorld();
        foreach (var tile in world.ConsumeDirtyRaisedFeatureTiles())
        {
            var chunkCoord = tile.ToChunkCoord(_runtime!.ChunkWidth, _runtime.ChunkHeight);
            if (!_chunkVisuals.TryGetValue(chunkCoord, out var visual))
            {
                continue;
            }

            RefreshRaisedFeatureTile(tile, visual);
        }
    }

    private void RefreshRaisedFeatureTile(WorldTileCoord tile, ChunkVisualState visual)
    {
        var key = $"{tile.X},{tile.Y}";
        if (visual.RaisedSprites.Remove(key, out var existing))
        {
            existing.QueueFree();
        }

        if (!_worldFacade!.GetActiveWorld().TryGetRaisedFeature(tile, _runtime!.ChunkWidth, _runtime.ChunkHeight, out var featureId, out var variantIndex))
        {
            return;
        }

        var raisedFeature = _runtime.Content.RaisedFeatures.Get(featureId!.Value);
        var sprite = new Sprite2D
        {
            Name = $"Raised_{tile.X}_{tile.Y}",
            Centered = false,
            Texture = ResolveRaisedFeatureTexture(raisedFeature, variantIndex),
            Position = new Vector2(tile.X * TileSize, tile.Y * TileSize),
            ZIndex = 2
        };
        visual.RaisedFeatureRoot.AddChild(sprite);
        visual.RaisedSprites[key] = sprite;
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
        var activeWorld = _runtime!.WorldState.GetActiveWorld();
        var aliveIds = _runtime.WorldState.Entities.Where(entity => !entity.Removed).Select(entity => entity.Id).ToHashSet();
        foreach (var removedId in _entitySprites.Keys.Where(id => !aliveIds.Contains(id)).ToArray())
        {
            _entitySprites[removedId].QueueFree();
            _entitySprites.Remove(removedId);
            _entitySpriteChunks.Remove(removedId);
        }

        foreach (var entity in _runtime.WorldState.Entities.Where(entity => !entity.Removed))
        {
            if (!_chunkVisuals.TryGetValue(entity.ChunkCoord, out var chunkVisual))
            {
                continue;
            }

            if (!_entitySprites.TryGetValue(entity.Id, out var sprite))
            {
                sprite = new Sprite2D { Centered = false };
                chunkVisual.EntityRoot.AddChild(sprite);
                _entitySprites.Add(entity.Id, sprite);
                _entitySpriteChunks[entity.Id] = entity.ChunkCoord;
            }
            else if (_entitySpriteChunks[entity.Id] != entity.ChunkCoord)
            {
                sprite.Reparent(chunkVisual.EntityRoot);
                _entitySpriteChunks[entity.Id] = entity.ChunkCoord;
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
            sprite.Modulate = entity.HitFlashSeconds > 0f ? new Color(1f, 0.65f, 0.65f, 1f) : Colors.White;
            sprite.ZIndex = ResolveZIndex(entity);
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

    private Texture2D ResolveRaisedFeatureTexture(RaisedFeatureDef raisedFeatureDef, byte variantIndex)
    {
        return ResolveCachedTexture($"raised:{raisedFeatureDef.Id.Value}:{variantIndex}", () => _textureLoader.LoadRaisedFeature(raisedFeatureDef, variantIndex).Texture);
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

    private int ResolveZIndex(WorldEntityState entity)
    {
        return entity.Kind switch
        {
            WorldEntityKind.Placeable when _runtime!.Content.Placeables.Get(entity.DefinitionId).IsGroundCover => 1,
            WorldEntityKind.Placeable => 3,
            WorldEntityKind.ResourceNode => 4,
            WorldEntityKind.Creature => 5,
            WorldEntityKind.ItemDrop => 6,
            _ => 2
        };
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

    private sealed record ChunkVisualState(
        Node2D TerrainRoot,
        Node2D RaisedFeatureRoot,
        Node2D EntityRoot,
        Dictionary<string, Sprite2D> RaisedSprites)
    {
        public ChunkVisualState(Node2D terrainRoot, Node2D raisedFeatureRoot, Node2D entityRoot)
            : this(terrainRoot, raisedFeatureRoot, entityRoot, [])
        {
        }
    }
}
