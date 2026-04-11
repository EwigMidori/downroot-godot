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
    private ProgressBar? _healthBar;
    private ProgressBar? _hungerBar;
    private Label? _promptLabel;
    private Label? _timeLabel;
    private Label? _hotbarLabel;
    private Label? _inventoryLabel;
    private VBoxContainer? _craftingPanel;
    private ColorRect? _nightOverlay;
    private string _lastFacing = "down";

    public override void _Ready()
    {
        ConfigureInputMap();

        _runtime = new GameBootstrapper().Bootstrap();
        _simulation = new GameSimulation(_runtime);
        _inputService = new GodotInputService(() =>
        {
            var pointer = GetGlobalMousePosition();
            return new NumericsVector2(pointer.X, pointer.Y);
        });
        _textureLoader = new TextureContentLoader(new PackPathResolver());
        _animationFactory = new PlayerAnimationFactory(new PackPathResolver());

        _terrainLayer = new Node2D { Name = "TerrainLayer" };
        _entityLayer = new Node2D { Name = "EntityLayer" };
        AddChild(_terrainLayer);
        AddChild(_entityLayer);

        BuildStaticTerrain();
        CreatePlayer();
        CreateHud();
        ValidateContentLoads();
        RedrawEntities();
        RefreshHud();
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

        RedrawEntities();
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
            AnchorBottom = 1,
            OffsetRight = 0,
            OffsetBottom = 0
        };
        canvas.AddChild(_nightOverlay);

        var root = new MarginContainer
        {
            AnchorRight = 1,
            AnchorBottom = 1,
            OffsetLeft = 8,
            OffsetTop = 8,
            OffsetRight = -8,
            OffsetBottom = -8,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        canvas.AddChild(root);

        var stack = new VBoxContainer();
        root.AddChild(stack);

        _timeLabel = new Label();
        stack.AddChild(_timeLabel);

        _healthBar = new ProgressBar { MaxValue = _runtime!.BootstrapConfig.MaxHealth, CustomMinimumSize = new Vector2(220, 18) };
        stack.AddChild(_healthBar);

        _hungerBar = new ProgressBar { MaxValue = _runtime.BootstrapConfig.MaxHunger, CustomMinimumSize = new Vector2(220, 18) };
        stack.AddChild(_hungerBar);

        _promptLabel = new Label();
        stack.AddChild(_promptLabel);

        _hotbarLabel = new Label();
        stack.AddChild(_hotbarLabel);

        _inventoryLabel = new Label();
        stack.AddChild(_inventoryLabel);

        _craftingPanel = new VBoxContainer();
        stack.AddChild(_craftingPanel);
    }

    private void ValidateContentLoads()
    {
        var report = new ContentLoadReport();
        report.AddSuccess(_textureLoader!.LoadTerrain(_runtime!.Content.Terrains.Get(new ContentId("basegame:grass"))).ContentId, "terrain");
        report.AddSuccess(_textureLoader.LoadTerrain(_runtime.Content.Terrains.Get(new ContentId("basegame:dirt"))).ContentId, "terrain");
        report.AddSuccess(_textureLoader.LoadItem(_runtime.Content.Items.Get(new ContentId("basegame:stone"))).ContentId, "item");
        report.AddSuccess(_textureLoader.LoadPlaceable(_runtime.Content.Placeables.Get(new ContentId("basegame:workbench"))).ContentId, "placeable");
    }

    private void RedrawEntities()
    {
        foreach (var child in _entityLayer!.GetChildren())
        {
            child.QueueFree();
        }

        foreach (var entity in _runtime!.WorldState.Entities.Where(entity => !entity.Removed))
        {
            Texture2D texture;
            Vector2 position = ToGodot(entity.Position);

            switch (entity.Kind)
            {
                case WorldEntityKind.ResourceNode:
                    texture = _textureLoader!.LoadResourceNode(_runtime.Content.ResourceNodes.Get(entity.DefinitionId)).Texture;
                    break;
                case WorldEntityKind.Placeable:
                    texture = _textureLoader!.LoadPlaceable(_runtime.Content.Placeables.Get(entity.DefinitionId)).Texture;
                    break;
                case WorldEntityKind.Creature:
                    texture = _textureLoader!.LoadCreature(_runtime.Content.Creatures.Get(entity.DefinitionId)).Texture;
                    break;
                case WorldEntityKind.ItemDrop:
                    texture = _textureLoader!.LoadItem(_runtime.Content.Items.Get(entity.DefinitionId)).Texture;
                    break;
                default:
                    continue;
            }

            var sprite = new Sprite2D
            {
                Texture = texture,
                Position = position,
                Centered = false,
                FlipH = entity.Kind == WorldEntityKind.Creature && entity.Position.X > _runtime.Player.Position.X
            };
            _entityLayer.AddChild(sprite);
        }
    }

    private void RefreshHud()
    {
        _healthBar!.Value = _runtime!.Player.Survival.Health;
        _healthBar.TooltipText = $"HP {_runtime.Player.Survival.Health}/{_runtime.Player.Survival.MaxHealth}";

        _hungerBar!.Value = _runtime.Player.Survival.Hunger;
        _hungerBar.TooltipText = $"Hunger {_runtime.Player.Survival.Hunger}/{_runtime.Player.Survival.MaxHunger}";

        var isNight = _runtime.WorldState.IsNight(_runtime.BootstrapConfig.DayLengthSeconds);
        _timeLabel!.Text = isNight ? "Night: worms are active" : "Daytime";
        _nightOverlay!.Color = new Color(0.03f, 0.05f, 0.15f, isNight ? 0.35f : 0f);
        _promptLabel!.Text = string.IsNullOrWhiteSpace(_runtime.WorldState.InteractionPrompt)
            ? "[F] Interact  [Q] Consume  [Tab] Inventory  [C] Craft"
            : $"{_runtime.WorldState.InteractionPrompt}  [Q] Consume  [Tab] Inventory  [C] Craft";

        _hotbarLabel!.Text = BuildHotbarText();
        _inventoryLabel!.Visible = _runtime.WorldState.InventoryVisible;
        _inventoryLabel.Text = _runtime.WorldState.InventoryVisible ? BuildInventoryText() : string.Empty;
        RebuildCraftingPanel();
    }

    private void RebuildCraftingPanel()
    {
        foreach (var child in _craftingPanel!.GetChildren())
        {
            child.QueueFree();
        }

        _craftingPanel.Visible = _runtime!.WorldState.CraftingVisible;
        if (!_craftingPanel.Visible)
        {
            return;
        }

        _craftingPanel.AddChild(new Label
        {
            Text = _runtime.WorldState.ActiveStationKey is null ? "Handcraft" : $"Station: {_runtime.WorldState.ActiveStationKey}"
        });

        foreach (var recipe in _simulation!.GetAvailableRecipes())
        {
            var button = new Button
            {
                Text = $"{recipe.DisplayName} ({string.Join(", ", recipe.Ingredients.Select(i => $"{i.ItemId.Value.Split(':')[1]} x{i.Amount}"))})"
            };
            button.Pressed += () => _simulation.Craft(recipe.Id);
            _craftingPanel.AddChild(button);
        }
    }

    private string BuildHotbarText()
    {
        var lines = new List<string> { "Hotbar" };
        for (var index = 0; index < _runtime!.Player.HotbarSize; index++)
        {
            var slot = _runtime.Player.Inventory.Slots[index];
            var selected = index == _runtime.Player.SelectedHotbarIndex ? ">" : " ";
            var text = slot.ItemId is null ? "(empty)" : $"{_runtime.Content.Items.Get(slot.ItemId.Value).DisplayName} x{slot.Quantity}";
            lines.Add($"{selected}{index + 1}: {text}");
        }

        return string.Join('\n', lines);
    }

    private string BuildInventoryText()
    {
        var lines = new List<string> { "Backpack" };
        for (var index = 0; index < _runtime!.Player.Inventory.Slots.Count; index++)
        {
            var slot = _runtime.Player.Inventory.Slots[index];
            var text = slot.ItemId is null ? "(empty)" : $"{_runtime.Content.Items.Get(slot.ItemId.Value).DisplayName} x{slot.Quantity}";
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
}
