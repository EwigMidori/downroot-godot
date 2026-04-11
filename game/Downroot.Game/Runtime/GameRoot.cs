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
    private GameSimulation? _simulation;
    private IInputService? _inputService;
    private TextureContentLoader? _textureLoader;
    private PlayerAnimationFactory? _animationFactory;

    private Node2D? _terrainLayer;
    private Node2D? _entityLayer;
    private CharacterBody2D? _playerBody;
    private AnimatedSprite2D? _playerSprite;

    private CanvasLayer? _bootCanvas;
    private Label? _bootLabel;

    private ProgressBar? _healthBar;
    private ProgressBar? _hungerBar;
    private Label? _promptLabel;
    private Label? _timeLabel;
    private Label? _hintLabel;
    private RichTextLabel? _hotbarLabel;
    private RichTextLabel? _inventoryLabel;
    private VBoxContainer? _craftingPanel;
    private ColorRect? _nightOverlay;
    private ProgressBar? _destroyBar;

    private readonly Dictionary<EntityId, Sprite2D> _entitySprites = [];
    private string? _craftingPanelStateKey;
    private string _lastFacing = "down";

    public override void _Ready()
    {
        try
        {
            EnsureBootOverlay();
            UpdateBootStatus("Configuring input");
            ConfigureInputMap();

            UpdateBootStatus("Bootstrapping runtime");
            _runtime = new GameBootstrapper().Bootstrap();
            _simulation = new GameSimulation(_runtime);
            _inputService = new GodotInputService(() =>
            {
                var pointer = GetGlobalMousePosition();
                return new NumericsVector2(pointer.X, pointer.Y);
            });

            UpdateBootStatus("Resolving content root");
            var packPathResolver = new PackPathResolver();
            _textureLoader = new TextureContentLoader(packPathResolver);
            _animationFactory = new PlayerAnimationFactory(packPathResolver);
            GD.Print($"Content root resolved. Example grass path: {packPathResolver.ResolveAbsolutePath("packs/basegame/assets/world/terrain/ground/grass.png")}");

            _terrainLayer = new Node2D { Name = "TerrainLayer" };
            _entityLayer = new Node2D { Name = "EntityLayer" };
            AddChild(_terrainLayer);
            AddChild(_entityLayer);

            UpdateBootStatus("Building terrain");
            BuildStaticTerrain();
            UpdateBootStatus("Creating player");
            CreatePlayer();
            UpdateBootStatus("Creating HUD");
            CreateHud();
            UpdateBootStatus("Validating content");
            ValidateContentLoads();
            UpdateBootStatus("Drawing entities");
            SynchronizeEntities();
            RefreshHud();
            UpdateBootStatus("Ready");
            ClearBootOverlay();
        }
        catch (Exception exception)
        {
            GD.PushError(exception.ToString());
            ShowStartupError(exception);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_runtime is null || _simulation is null || _inputService is null || _playerBody is null || _playerSprite is null)
        {
            return;
        }

        var frame = _inputService.CaptureFrame();
        _simulation.Tick((float)delta, frame);

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
        RefreshHud();
    }

    private void BuildStaticTerrain()
    {
        var defaultTerrainDef = _runtime!.Content.Terrains.Get(_runtime.BootstrapConfig.DefaultTerrainId);
        var variantTerrainDef = _runtime.Content.Terrains.Get(_runtime.BootstrapConfig.DebugTerrainVariantId);
        var defaultTexture = _textureLoader!.LoadTerrain(defaultTerrainDef).Texture;
        var variantTexture = _textureLoader.LoadTerrain(variantTerrainDef).Texture;

        for (var y = 0; y < _runtime.World.Surface.Height; y++)
        {
            for (var x = 0; x < _runtime.World.Surface.Width; x++)
            {
                var terrainId = _runtime.World.Surface.GetTerrainId(x, y) ?? defaultTerrainDef.Id;
                var sprite = new Sprite2D
                {
                    Centered = false,
                    Texture = terrainId == variantTerrainDef.Id ? variantTexture : defaultTexture,
                    Position = new Vector2(x * TileSize, y * TileSize)
                };
                _terrainLayer!.AddChild(sprite);
            }
        }
    }

    private void CreatePlayer()
    {
        var creature = _runtime!.Content.Creatures.Get(_runtime.BootstrapConfig.PlayerCreatureId);
        var frames = _animationFactory!.Create(creature);

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

    private void CreateHud()
    {
        var canvas = new CanvasLayer();
        AddChild(canvas);

        _nightOverlay = new ColorRect
        {
            Color = new Color(0.03f, 0.05f, 0.15f, 0f),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            AnchorRight = 1,
            AnchorBottom = 1
        };
        canvas.AddChild(_nightOverlay);

        var topLeftPanel = new PanelContainer
        {
            Position = new Vector2(12, 12),
            Size = new Vector2(250, 96),
            Modulate = new Color(1f, 1f, 1f, 0.92f)
        };
        canvas.AddChild(topLeftPanel);

        var topLeftContent = new VBoxContainer();
        topLeftPanel.AddChild(topLeftContent);

        _timeLabel = new Label();
        topLeftContent.AddChild(_timeLabel);

        _healthBar = new ProgressBar { MaxValue = _runtime!.BootstrapConfig.MaxHealth, CustomMinimumSize = new Vector2(220, 16), ShowPercentage = false };
        topLeftContent.AddChild(_healthBar);

        _hungerBar = new ProgressBar { MaxValue = _runtime.BootstrapConfig.MaxHunger, CustomMinimumSize = new Vector2(220, 16), ShowPercentage = false };
        topLeftContent.AddChild(_hungerBar);

        _hintLabel = new Label
        {
            Text = "WASD Move  F Interact  LMB Break  RMB Place  Q Eat  C Craft  Tab Bag",
            Position = new Vector2(12, 114)
        };
        canvas.AddChild(_hintLabel);

        _promptLabel = new Label
        {
            Position = new Vector2(12, 138)
        };
        canvas.AddChild(_promptLabel);

        _destroyBar = new ProgressBar
        {
            ShowPercentage = false,
            Visible = false,
            CustomMinimumSize = new Vector2(180, 12),
            Position = new Vector2(280, 12),
            MaxValue = 1
        };
        canvas.AddChild(_destroyBar);

        var hotbarPanel = new PanelContainer
        {
            AnchorLeft = 0.5f,
            AnchorTop = 1,
            AnchorRight = 0.5f,
            AnchorBottom = 1,
            OffsetLeft = -210,
            OffsetTop = -88,
            OffsetRight = 210,
            OffsetBottom = -12,
            Modulate = new Color(1f, 1f, 1f, 0.94f)
        };
        canvas.AddChild(hotbarPanel);

        _hotbarLabel = new RichTextLabel
        {
            FitContent = true,
            BbcodeEnabled = true,
            ScrollActive = false
        };
        hotbarPanel.AddChild(_hotbarLabel);

        var rightPanel = new PanelContainer
        {
            AnchorLeft = 1,
            AnchorTop = 0,
            AnchorRight = 1,
            AnchorBottom = 0,
            OffsetLeft = -280,
            OffsetTop = 12,
            OffsetRight = -12,
            OffsetBottom = 320,
            Modulate = new Color(1f, 1f, 1f, 0.94f)
        };
        canvas.AddChild(rightPanel);

        var rightStack = new VBoxContainer();
        rightPanel.AddChild(rightStack);

        _inventoryLabel = new RichTextLabel
        {
            FitContent = false,
            ScrollActive = true,
            BbcodeEnabled = true,
            CustomMinimumSize = new Vector2(248, 150),
            Visible = false
        };
        rightStack.AddChild(_inventoryLabel);

        _craftingPanel = new VBoxContainer();
        rightStack.AddChild(_craftingPanel);
    }

    private void ValidateContentLoads()
    {
        var report = new ContentLoadReport();
        report.AddSuccess(_textureLoader!.LoadTerrain(_runtime!.Content.Terrains.Get(new ContentId("basegame:grass"))).ContentId, "terrain");
        report.AddSuccess(_textureLoader.LoadTerrain(_runtime.Content.Terrains.Get(new ContentId("basegame:dirt"))).ContentId, "terrain");
        report.AddSuccess(_textureLoader.LoadItem(_runtime.Content.Items.Get(new ContentId("basegame:stone"))).ContentId, "item");
        report.AddSuccess(_textureLoader.LoadPlaceable(_runtime.Content.Placeables.Get(new ContentId("basegame:workbench"))).ContentId, "placeable");
        GD.Print($"Terrain tiles: {_runtime.World.Surface.Width}x{_runtime.World.Surface.Height}, entities: {_runtime.WorldState.Entities.Count}");
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

            sprite.Texture = ResolveEntityTexture(entity);
            sprite.Position = ToGodot(entity.Position);
            sprite.FlipH = entity.Kind == WorldEntityKind.Creature && entity.Position.X > _runtime.Player.Position.X;
        }
    }

    private Texture2D ResolveEntityTexture(WorldEntityState entity)
    {
        return entity.Kind switch
        {
            WorldEntityKind.ResourceNode => _textureLoader!.LoadResourceNode(_runtime!.Content.ResourceNodes.Get(entity.DefinitionId)).Texture,
            WorldEntityKind.Placeable => _textureLoader!.LoadPlaceable(_runtime!.Content.Placeables.Get(entity.DefinitionId), entity.OpenState).Texture,
            WorldEntityKind.Creature => _textureLoader!.LoadCreature(_runtime!.Content.Creatures.Get(entity.DefinitionId)).Texture,
            WorldEntityKind.ItemDrop => _textureLoader!.LoadItem(_runtime!.Content.Items.Get(entity.DefinitionId)).Texture,
            _ => throw new InvalidOperationException($"Unsupported entity kind '{entity.Kind}'.")
        };
    }

    private void RefreshHud()
    {
        _healthBar!.Value = _runtime!.Player.Survival.Health;
        _healthBar.TooltipText = $"HP {_runtime.Player.Survival.Health}/{_runtime.Player.Survival.MaxHealth}";

        _hungerBar!.Value = _runtime.Player.Survival.Hunger;
        _hungerBar.TooltipText = $"Hunger {_runtime.Player.Survival.Hunger}/{_runtime.Player.Survival.MaxHunger}";

        var isNight = _runtime.WorldState.IsNight(_runtime.BootstrapConfig.DayLengthSeconds);
        _timeLabel!.Text = isNight ? "Night: worms are active" : "Daytime: gather and build";
        _nightOverlay!.Color = new Color(0.03f, 0.05f, 0.15f, isNight ? 0.35f : 0f);
        _promptLabel!.Text = string.IsNullOrWhiteSpace(_runtime.WorldState.InteractionPrompt)
            ? "Find trees, stones, and berries. Craft a workbench before nightfall."
            : _runtime.WorldState.InteractionPrompt;

        _destroyBar!.Visible = _runtime.WorldState.DestroyProgress01 > 0f;
        _destroyBar.Value = _runtime.WorldState.DestroyProgress01;

        _hotbarLabel!.Text = BuildHotbarText();
        _inventoryLabel!.Visible = _runtime.WorldState.InventoryVisible;
        _inventoryLabel.Text = _runtime.WorldState.InventoryVisible ? BuildInventoryText() : string.Empty;
        RebuildCraftingPanel();
    }

    private void RebuildCraftingPanel()
    {
        var isVisible = _runtime!.WorldState.CraftingVisible;
        _craftingPanel!.Visible = isVisible;
        if (!isVisible)
        {
            if (_craftingPanelStateKey is not null)
            {
                ClearCraftingPanel();
                _craftingPanelStateKey = null;
            }
            return;
        }

        var recipes = _simulation!.GetAvailableRecipes();
        var panelStateKey = string.Join('|', new[]
        {
            _runtime.WorldState.ActiveStationKey ?? "handcraft",
            string.Join(',', recipes.Select(recipe => recipe.Id.Value))
        });

        if (_craftingPanelStateKey == panelStateKey)
        {
            return;
        }

        ClearCraftingPanel();
        _craftingPanelStateKey = panelStateKey;

        _craftingPanel.AddChild(new Label
        {
            Text = _runtime.WorldState.ActiveStationKey is null ? "Handcraft" : $"Workbench Recipes"
        });

        foreach (var recipe in recipes)
        {
            var recipeId = recipe.Id;
            var button = new Button
            {
                Text = $"{recipe.DisplayName} ({string.Join(", ", recipe.Ingredients.Select(i => $"{ShortName(i.ItemId)} x{i.Amount}"))})"
            };
            button.Pressed += () =>
            {
                if (_simulation.Craft(recipeId))
                {
                    _craftingPanelStateKey = null;
                    RefreshHud();
                }
            };
            _craftingPanel.AddChild(button);
        }
    }

    private void ClearCraftingPanel()
    {
        foreach (var child in _craftingPanel!.GetChildren())
        {
            child.QueueFree();
        }
    }

    private string BuildHotbarText()
    {
        var parts = new List<string>();
        for (var index = 0; index < _runtime!.Player.HotbarSize; index++)
        {
            var slot = _runtime.Player.Inventory.Slots[index];
            var prefix = index == _runtime.Player.SelectedHotbarIndex ? "[color=yellow]>" : "[color=gray]";
            var suffix = "[/color]";
            var text = slot.ItemId is null ? "Empty" : $"{ShortName(slot.ItemId.Value)} x{slot.Quantity}";
            parts.Add($"{prefix}{index + 1} {text}{suffix}");
        }

        return string.Join("   ", parts);
    }

    private string BuildInventoryText()
    {
        var lines = new List<string> { "[b]Backpack[/b]" };
        for (var index = 0; index < _runtime!.Player.Inventory.Slots.Count; index++)
        {
            var slot = _runtime.Player.Inventory.Slots[index];
            var text = slot.ItemId is null ? "Empty" : $"{ShortName(slot.ItemId.Value)} x{slot.Quantity}";
            lines.Add($"{index + 1:00}: {text}");
        }

        return string.Join('\n', lines);
    }

    private static void ConfigureInputMap()
    {
        EnsureAction("move_left", Key.A);
        EnsureAction("move_right", Key.D);
        EnsureAction("move_up", Key.W);
        EnsureAction("move_down", Key.S);
        EnsureAction("interact", Key.F);
        EnsureAction("toggle_inventory", Key.Tab);
        EnsureAction("toggle_crafting", Key.C);
        EnsureAction("consume_selected", Key.Q);

        EnsureMouseAction("hotbar_next", MouseButton.WheelDown);
        EnsureMouseAction("hotbar_prev", MouseButton.WheelUp);

        EnsureAction("hotbar_1", Key.Key1);
        EnsureAction("hotbar_2", Key.Key2);
        EnsureAction("hotbar_3", Key.Key3);
        EnsureAction("hotbar_4", Key.Key4);
        EnsureAction("hotbar_5", Key.Key5);
        EnsureAction("hotbar_6", Key.Key6);
        EnsureAction("hotbar_7", Key.Key7);
        EnsureAction("hotbar_8", Key.Key8);
    }

    private static void EnsureAction(string actionName, Key key)
    {
        if (!InputMap.HasAction(actionName))
        {
            InputMap.AddAction(actionName);
        }

        if (InputMap.ActionGetEvents(actionName).OfType<InputEventKey>().Any(existing => existing.PhysicalKeycode == key))
        {
            return;
        }

        InputMap.ActionAddEvent(actionName, new InputEventKey { PhysicalKeycode = key });
    }

    private static void EnsureMouseAction(string actionName, MouseButton button)
    {
        if (!InputMap.HasAction(actionName))
        {
            InputMap.AddAction(actionName);
        }

        if (InputMap.ActionGetEvents(actionName).OfType<InputEventMouseButton>().Any(existing => existing.ButtonIndex == button))
        {
            return;
        }

        InputMap.ActionAddEvent(actionName, new InputEventMouseButton { ButtonIndex = button });
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

    private void ShowStartupError(Exception exception)
    {
        EnsureBootOverlay();
        if (_bootLabel is not null)
        {
            _bootLabel.Text = $"Startup failed:\n{exception}";
        }
    }

    private void EnsureBootOverlay()
    {
        if (_bootLabel is not null)
        {
            return;
        }

        _bootCanvas = new CanvasLayer();
        AddChild(_bootCanvas);

        _bootCanvas.AddChild(new ColorRect
        {
            Color = new Color(0.04f, 0.05f, 0.07f, 0.92f),
            AnchorRight = 1,
            AnchorBottom = 1
        });

        _bootLabel = new Label
        {
            Text = "Booting Downroot...",
            Position = new Vector2(12, 12)
        };
        _bootCanvas.AddChild(_bootLabel);
    }

    private void ClearBootOverlay()
    {
        _bootCanvas?.QueueFree();
        _bootCanvas = null;
        _bootLabel = null;
    }

    private void UpdateBootStatus(string message)
    {
        GD.Print($"[Boot] {message}");
        if (_bootLabel is not null)
        {
            _bootLabel.Text = $"Booting Downroot...\n{message}";
        }
    }

    private static string ShortName(ContentId id) => id.Value.Split(':')[1].Replace('_', ' ');
}
