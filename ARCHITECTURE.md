# Downroot Architecture Draft

这份文档的目标是定义一个适合 `Godot + C#` 的高层架构草案。重点不是具体语法，而是先把模块边界、扩展点、注册流程和关键对象关系定下来。

设计目标：

- `v0.1` 就按 Mod-ready 架构开发。
- `basegame` 不是特殊逻辑，而是默认内容包。
- 内容、世界生成、交互、配方都通过注册和接口扩展。
- UI 只消费查询接口和 ViewModel，不直接耦合底层实现。
- 后续增加 Mod 时，主要补加载器和产品能力，而不是重写核心系统。

## 1. 设计原则

### 1.1 分层依赖

建议依赖方向如下：

`Core -> Content -> World -> Gameplay -> UI`

附加层：

`Mods -> Content`

约束：

- `Core` 不依赖任何具体内容。
- `Content` 不依赖 Godot 场景层业务。
- `World` 不知道 UI。
- `Gameplay` 不直接依赖 `basegame` 的具体资源路径。
- `UI` 不直接修改世界内部数据结构。

### 1.2 Basegame 也是内容包

这是整套设计里最关键的一条：

- `basegame` 必须通过和未来 Mod 相同的注册入口装入。
- 基础物品、基础地形、基础配方、基础生物都来自 `basegame` 内容包。
- 游戏内核只识别内容 ID 和注册表，不识别“这是基础游戏特例”。

这样后续加入 Mod 时，新增内容只是“再装一个内容包”，而不是“给核心逻辑打补丁”。

### 1.3 数据驱动优先

下列内容尽量数据驱动：

- 物品定义
- 可放置物定义
- 配方定义
- 生物定义
- 资源掉落表
- 世界生成参数

下列内容允许代码扩展：

- 特殊交互行为
- 复杂 AI
- 特殊世界生成 pass
- 复杂状态机

## 2. 模块划分

## 2.1 Core

职责：

- 应用启动与生命周期
- 事件总线
- 输入命令抽象
- 随机种子与哈希
- 日志
- 基础配置
- 通用 ID 类型与结果类型

建议类型：

```csharp
public readonly record struct ContentId(string Value);
public readonly record struct EntityId(long Value);
public readonly record struct ChunkCoord(int X, int Y);
public readonly record struct TileCoord(int X, int Y);

public interface IGameLogger
{
    void Info(string message);
    void Warn(string message);
    void Error(string message);
}

public interface IEventBus
{
    void Publish<TEvent>(TEvent evt);
    IDisposable Subscribe<TEvent>(Action<TEvent> handler);
}
```

输入建议不要让玩家控制脚本直接读 Godot 键位，而是先抽象成动作命令：

```csharp
public enum InputActionId
{
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
    Sprint,
    Interact,
    BreakHold,
    Place,
    OpenCrafting,
    Hotbar1,
    Hotbar2,
    Hotbar3,
    Hotbar4,
    Hotbar5,
    Hotbar6,
    Hotbar7,
    Hotbar8
}

public interface IInputService
{
    bool IsPressed(InputActionId action);
    bool IsJustPressed(InputActionId action);
    Vector2 GetMovementVector();
}
```

## 2.2 Content

职责：

- 内容定义类型
- 内容包加载
- 注册中心
- 内容引用解析

### 2.2.1 内容定义

建议先区分“定义”和“运行时实例”。

```csharp
public abstract class ContentDef
{
    public ContentId Id { get; init; }
    public string DisplayName { get; init; } = "";
    public string SourcePack { get; init; } = "";
}

public sealed class ItemDef : ContentDef
{
    public int MaxStack { get; init; } = 64;
    public string IconPath { get; init; } = "";
    public ItemTags Tags { get; init; } = ItemTags.None;
    public FoodComponentDef? Food { get; init; }
    public PlaceableRef? Placeable { get; init; }
    public ToolComponentDef? Tool { get; init; }
}

public sealed class PlaceableDef : ContentDef
{
    public string ScenePath { get; init; } = "";
    public Int2 Footprint { get; init; } = Int2.One;
    public bool IsDestructible { get; init; }
    public float BreakSeconds { get; init; }
    public CollisionProfileDef Collision { get; init; } = CollisionProfileDef.Default;
}

public sealed class RecipeDef : ContentDef
{
    public RecipeStationKind Station { get; init; }
    public IReadOnlyList<ItemAmountDef> Inputs { get; init; } = Array.Empty<ItemAmountDef>();
    public IReadOnlyList<ItemAmountDef> Outputs { get; init; } = Array.Empty<ItemAmountDef>();
}

public sealed class CreatureDef : ContentDef
{
    public string ScenePath { get; init; } = "";
    public int MaxHealth { get; init; }
    public float MoveSpeed { get; init; }
    public AiArchetype Ai { get; init; }
    public IReadOnlyList<LootDropDef> LootTable { get; init; } = Array.Empty<LootDropDef>();
}

public sealed class TerrainDef : ContentDef
{
    public string TileSourcePath { get; init; } = "";
    public TerrainKind Kind { get; init; }
    public bool Walkable { get; init; } = true;
}
```

### 2.2.2 内容包接口

```csharp
public interface IContentPack
{
    string PackId { get; }
    string Version { get; }
    void Register(IContentRegistrar registrar);
}

public interface IContentRegistrar
{
    void RegisterItem(ItemDef def);
    void RegisterPlaceable(PlaceableDef def);
    void RegisterRecipe(RecipeDef def);
    void RegisterCreature(CreatureDef def);
    void RegisterTerrain(TerrainDef def);
    void RegisterWorldGenPass(IWorldGenPass pass);
}
```

说明：

- `basegame` 实现一个 `BaseGameContentPack`。
- 后续 `portal mod` 也是一个 `PortalModContentPack`。
- 核心启动流程只认 `IContentPack`，不区分 basegame 和 mod。

### 2.2.3 注册表

```csharp
public interface IRegistry<TDef> where TDef : ContentDef
{
    void Add(TDef def);
    TDef Get(ContentId id);
    bool TryGet(ContentId id, out TDef? def);
    IReadOnlyCollection<TDef> All();
}

public sealed class ContentRegistrySet
{
    public IRegistry<ItemDef> Items { get; }
    public IRegistry<PlaceableDef> Placeables { get; }
    public IRegistry<RecipeDef> Recipes { get; }
    public IRegistry<CreatureDef> Creatures { get; }
    public IRegistry<TerrainDef> Terrains { get; }
    public IReadOnlyList<IWorldGenPass> WorldGenPasses { get; }
}
```

建议统一 ID 规范，例如：

```text
basegame:log
basegame:stone
basegame:workbench
portalmod:portal
portalmod:frost_core
```

## 2.3 World

职责：

- 格子与区块
- 地形层
- 可放置物层
- 掉落物层
- 世界查询
- 世界生成流水线

### 2.3.1 世界数据结构

```csharp
public sealed class WorldModel
{
    public int Seed { get; }
    public IChunkRepository Chunks { get; }
    public IEntityRepository Entities { get; }
}

public sealed class ChunkData
{
    public ChunkCoord Coord { get; init; }
    public TerrainCell[,] Terrain { get; init; } = default!;
    public List<PlacedEntityRecord> Placeables { get; } = new();
    public List<PickupRecord> Pickups { get; } = new();
    public bool IsGenerated { get; set; }
}

public readonly record struct TerrainCell(ContentId TerrainId, byte Variant);
```

这里建议把“地形”“放置物”“掉落物”分层，不要一开始就混成一个万能 tile。

### 2.3.2 世界生成

世界生成必须用 pass 流水线，不要写在一个 Godot Node 里。

```csharp
public interface IWorldGenPass
{
    string Name { get; }
    int Order { get; }
    void Apply(WorldGenContext context, ChunkData chunk);
}

public sealed class WorldGenContext
{
    public int Seed { get; }
    public ChunkCoord ChunkCoord { get; }
    public INoiseService Noise { get; }
    public IRandomService Random { get; }
    public ContentRegistrySet Registries { get; }
}
```

建议首批 pass：

- `GroundTerrainPass`
- `GrassDecorationPass`
- `TreeSpawnPass`
- `LooseStoneSpawnPass`
- `BerrySpawnPass`
- `WormSpawnPass`

后面再加：

- `OreClusterPass`
- `PortalSpawnPass`
- `DimensionalFragmentPass`

### 2.3.3 世界查询接口

```csharp
public interface IWorldQueryService
{
    TerrainCell GetTerrain(TileCoord tile);
    IEnumerable<WorldEntity> FindEntitiesInRadius(Vector2 worldPos, float radius);
    WorldEntity? FindNearestInteractable(Vector2 worldPos, float radius);
    bool CanPlace(ContentId placeableId, TileCoord tile);
}
```

## 2.4 Gameplay

职责：

- 玩家状态
- 背包
- 快捷栏
- 交互
- 采集
- 放置
- 合成
- 生存数值
- 战斗

### 2.4.1 玩家聚合

```csharp
public sealed class PlayerState
{
    public EntityId Id { get; init; }
    public Vector2 Position { get; set; }
    public int Health { get; set; } = 100;
    public float Hunger { get; set; } = 100f;
    public Inventory Inventory { get; } = new(16);
    public Hotbar Hotbar { get; } = new(8);
}
```

### 2.4.2 背包与物品堆

```csharp
public readonly record struct ItemStack(ContentId ItemId, int Amount);

public sealed class Inventory
{
    private readonly ItemStack?[] _slots;

    public Inventory(int size)
    {
        _slots = new ItemStack?[size];
    }

    public bool TryAdd(ItemStack stack);
    public bool HasItems(IEnumerable<ItemAmountDef> cost);
    public bool TryConsume(IEnumerable<ItemAmountDef> cost);
    public IReadOnlyList<ItemStack?> Slots => _slots;
}
```

### 2.4.3 交互接口

这是未来可扩展的关键点之一。

```csharp
public interface IInteractable
{
    InteractionPrompt GetPrompt(PlayerState player);
    InteractionResult Interact(InteractionContext context);
}

public interface IDestructible
{
    float BreakSeconds { get; }
    void ApplyBreakProgress(float seconds);
    DestroyResult Destroy(DestroyContext context);
}

public interface IPickupSource
{
    ItemStack GetPickup();
}
```

说明：

- 工作台、木箱、传送门、掉落物都可以实现 `IInteractable`。
- 树、石头、墙、工作台都可以实现 `IDestructible`。
- 这样不需要在玩家脚本里写 `if target is Workbench` 这种分支。

### 2.4.4 放置系统

```csharp
public interface IPlacementService
{
    bool CanPlace(PlayerState player, ContentId placeableId, TileCoord tile);
    PlacementResult Place(PlayerState player, ContentId placeableId, TileCoord tile);
}
```

### 2.4.5 合成系统

不要把配方硬编码在 UI 或工作台脚本里。

```csharp
public interface ICraftingService
{
    IReadOnlyList<RecipeDef> GetAvailableRecipes(PlayerState player, RecipeStationKind station);
    bool CanCraft(PlayerState player, ContentId recipeId, RecipeStationKind station);
    CraftResult Craft(PlayerState player, ContentId recipeId, RecipeStationKind station);
}
```

首版中：

- 手搓界面查询 `RecipeStationKind.Handcraft`
- 工作台界面查询 `RecipeStationKind.Workbench`
- 熔炉后面扩展 `RecipeStationKind.Furnace`

### 2.4.6 生存与时间

```csharp
public sealed class GameClock
{
    public double WorldHours { get; private set; }
    public float DayLengthHours { get; init; } = 24f;
    public TimeOfDayPhase CurrentPhase { get; private set; }

    public void Advance(double deltaSeconds);
}

public sealed class SurvivalSystem
{
    public void Tick(PlayerState player, float deltaSeconds, bool isMoving)
    {
        // v0.1 可以先做简单匀速扣饱腹
    }
}
```

### 2.4.7 战斗

首版先不要做复杂命中框架，但接口要留出来。

```csharp
public interface ICombatService
{
    void DealDamage(EntityId source, EntityId target, DamageInfo damage);
}

public readonly record struct DamageInfo(
    int Amount,
    DamageType Type,
    Vector2 Knockback);
```

## 2.5 UI

职责：

- HUD
- 快捷栏
- 背包界面
- 合成界面
- 交互提示
- 调试界面

核心原则：

- UI 只消费状态查询接口和 ViewModel。
- UI 不自己拼业务逻辑。

例如：

```csharp
public sealed class HudViewModel
{
    public int Health { get; init; }
    public float Hunger { get; init; }
    public IReadOnlyList<HotbarSlotViewModel> Hotbar { get; init; } = Array.Empty<HotbarSlotViewModel>();
    public string? InteractionPrompt { get; init; }
}

public interface IUiQueryService
{
    HudViewModel GetHud(PlayerState player);
    InventoryViewModel GetInventory(PlayerState player);
    CraftingPanelViewModel GetCraftingPanel(PlayerState player, RecipeStationKind station);
}
```

这样以后即使底层背包实现变了，UI 不需要跟着重写。

## 2.6 Mods

职责：

- 发现内容包
- 装配内容包
- 管理启停
- 处理依赖

但在 `v0.1` 阶段：

- 这一层可以只有接口和简化实现。
- 初期甚至可以手动把 `BaseGameContentPack` 注册到启动器里。
- 关键是启动流程要允许未来替换成“扫描目录并自动加载多个内容包”。

伪代码：

```csharp
public interface IContentPackLocator
{
    IReadOnlyList<IContentPack> DiscoverEnabledPacks();
}

public sealed class BootstrapPackLocator : IContentPackLocator
{
    public IReadOnlyList<IContentPack> DiscoverEnabledPacks()
    {
        return new IContentPack[]
        {
            new BaseGameContentPack()
        };
    }
}
```

后期可以换成：

- 从磁盘读取 pack 元数据
- 解析依赖
- 构建加载顺序
- 再统一注册

## 3. 启动流程

建议定义一个统一的 `GameBootstrapper`，不要把启动逻辑分散到多个场景脚本。

```csharp
public sealed class GameBootstrapper
{
    public GameRuntime Boot(GameBootConfig config)
    {
        var logger = new GodotLogger();
        var events = new EventBus();
        var registries = new ContentRegistrySet(...);

        IContentPackLocator locator = new BootstrapPackLocator();
        var packs = locator.DiscoverEnabledPacks();

        var registrar = new ContentRegistrar(registries, logger);
        foreach (var pack in packs)
        {
            pack.Register(registrar);
        }

        var world = new WorldModelFactory(registries).Create(config.Seed);
        var gameplay = new GameplayRuntime(world, registries, events, logger);
        var ui = new UiRuntime(gameplay, registries);

        return new GameRuntime(world, gameplay, ui, registries, logger, events);
    }
}
```

说明：

- 启动时先建基础服务。
- 再装内容包。
- 再建世界和 gameplay runtime。
- UI 最后绑定到运行时。

## 4. Basegame 示例

下面是 `basegame` 应该如何注册内容的高层样子。

```csharp
public sealed class BaseGameContentPack : IContentPack
{
    public string PackId => "basegame";
    public string Version => "0.1.0";

    public void Register(IContentRegistrar registrar)
    {
        registrar.RegisterTerrain(new TerrainDef
        {
            Id = new ContentId("basegame:grass"),
            DisplayName = "Grass",
            SourcePack = PackId,
            TileSourcePath = "basegame/assets/world/terrain/ground/grass.png",
            Kind = TerrainKind.Ground
        });

        registrar.RegisterItem(new ItemDef
        {
            Id = new ContentId("basegame:log"),
            DisplayName = "Log",
            SourcePack = PackId,
            IconPath = "basegame/assets/items/log.png",
            MaxStack = 64
        });

        registrar.RegisterPlaceable(new PlaceableDef
        {
            Id = new ContentId("basegame:workbench"),
            DisplayName = "Workbench",
            SourcePack = PackId,
            ScenePath = "res://scenes/placeables/workbench.tscn",
            IsDestructible = true,
            BreakSeconds = 1.5f
        });

        registrar.RegisterRecipe(new RecipeDef
        {
            Id = new ContentId("basegame:recipe_workbench"),
            DisplayName = "Workbench",
            SourcePack = PackId,
            Station = RecipeStationKind.Handcraft,
            Inputs = new[]
            {
                new ItemAmountDef(new ContentId("basegame:log"), 4),
                new ItemAmountDef(new ContentId("basegame:stone"), 1)
            },
            Outputs = new[]
            {
                new ItemAmountDef(new ContentId("basegame:workbench_item"), 1)
            }
        });

        registrar.RegisterWorldGenPass(new GroundTerrainPass());
        registrar.RegisterWorldGenPass(new TreeSpawnPass());
        registrar.RegisterWorldGenPass(new LooseStoneSpawnPass());
        registrar.RegisterWorldGenPass(new BerrySpawnPass());
        registrar.RegisterWorldGenPass(new WormSpawnPass());
    }
}
```

## 5. Portal Mod 示例

这部分不是现在就做，而是用来验证架构是否站得住。

```csharp
public sealed class PortalModContentPack : IContentPack
{
    public string PackId => "portalmod";
    public string Version => "0.1.0";

    public void Register(IContentRegistrar registrar)
    {
        registrar.RegisterTerrain(new TerrainDef
        {
            Id = new ContentId("portalmod:dimfrag"),
            DisplayName = "Dimensional Fragment",
            SourcePack = PackId,
            TileSourcePath = "portalmod/assets/world/terrain/ground/dimfrag.png",
            Kind = TerrainKind.Ground
        });

        registrar.RegisterPlaceable(new PlaceableDef
        {
            Id = new ContentId("portalmod:portal"),
            DisplayName = "Portal",
            SourcePack = PackId,
            ScenePath = "res://mods/portalmod/scenes/portal.tscn",
            IsDestructible = false
        });

        registrar.RegisterWorldGenPass(new PortalSpawnPass());
        registrar.RegisterWorldGenPass(new DimensionalPocketPass());
    }
}
```

如果这段在架构上能自然成立，就说明 `basegame` 和 `mod` 没有走两套系统。

## 6. Godot 层建议

虽然核心逻辑建议尽量做成普通 C# 类，但 Godot 层仍然需要明确职责。

建议：

- `GameRoot`：Godot 场景入口，持有 `GameRuntime`
- `WorldView`：根据 `WorldModel` 渲染地形、放置物、掉落物
- `PlayerNode`：只处理表现、动画、碰撞和输入桥接
- `HudController`：把 `ViewModel` 映射到 Godot UI

不要让这些节点直接承担：

- 配方注册
- 内容定义
- 世界生成逻辑
- 物品数据库

## 7. 首版推荐目录草案

这里先给一个逻辑目录，不强求你完全照抄：

```text
src/
  Downroot.Core/
    Events/
    Input/
    Logging/
    Random/
    Primitives/

  Downroot.Content/
    Definitions/
    Registries/
    Packs/
    Loading/

  Downroot.World/
    Chunks/
    Terrain/
    Entities/
    Generation/
    Queries/

  Downroot.Gameplay/
    Players/
    Inventory/
    Interaction/
    Crafting/
    Placement/
    Survival/
    Combat/

  Downroot.UI/
    ViewModels/
    Queries/
    Controllers/

  Downroot.Game/
    Bootstrap/
    Runtime/
    GodotAdapters/
```

如果一开始不想拆成多个 csproj，也建议至少按这个目录和命名空间分层。

## 8. 首版必须先定下来的接口

如果只能优先设计少数几个接口，我建议先定这几个：

1. `IContentPack`
2. `IContentRegistrar`
3. `IRegistry<T>`
4. `IWorldGenPass`
5. `IInteractable`
6. `IDestructible`
7. `ICraftingService`
8. `IPlacementService`
9. `IWorldQueryService`
10. `IUiQueryService`

这几个接口一旦定稳，后面大多数系统都能围绕它们展开。

## 9. 当前建议

下一步最合理的不是立刻把所有接口都实现，而是继续补一份更实用的文档：

- 把 `v0.1` 涉及的内容定义列成表
- 把首批 registry 项列出来
- 把启动流程细化成“初始化时序图”
- 把世界生成 pass 顺序具体化

也就是说，先把抽象边界定住，再进入真正编码。
