using Downroot.Core.Content;
using Downroot.Core.Definitions;
using Downroot.Core.Gameplay;
using Downroot.Core.Ids;
using Downroot.Core.World;

namespace Downroot.Content.Packs;

public sealed class BaseGameContentPack : IContentPack
{
    public const string Id = "basegame";

    public string PackId => Id;

    public void Register(IContentRegistrar registrar)
    {
        var grassId = new ContentId("basegame:grass");
        var dirtId = new ContentId("basegame:dirt");

        var logItemId = new ContentId("basegame:log");
        var stoneItemId = new ContentId("basegame:stone");
        var blueberryItemId = new ContentId("basegame:blueberry");
        var workbenchItemId = new ContentId("basegame:workbench_item");
        var torchItemId = new ContentId("basegame:torch");
        var chestItemId = new ContentId("basegame:wooden_chest_item");
        var doorItemId = new ContentId("basegame:wooden_door_item");
        var fenceItemId = new ContentId("basegame:wooden_fence_item");

        var workbenchPlaceableId = new ContentId("basegame:workbench");
        var torchPlaceableId = new ContentId("basegame:torch_placeable");
        var chestPlaceableId = new ContentId("basegame:wooden_chest");
        var doorPlaceableId = new ContentId("basegame:wooden_door");
        var fencePlaceableId = new ContentId("basegame:wooden_fence");

        var playerId = new ContentId("basegame:player_human");
        var wormId = new ContentId("basegame:worm");

        var treeNodeId = new ContentId("basegame:tree_bright");
        var stoneNodeId = new ContentId("basegame:stone_node");
        var blueberryNodeId = new ContentId("basegame:blueberry_bush");

        registrar.RegisterTerrain(new TerrainDef(grassId, "Grass", PackId, "packs/basegame/assets/world/terrain/ground/grass.png", 32, 32, 0, 0));
        registrar.RegisterTerrain(new TerrainDef(dirtId, "Dirt", PackId, "packs/basegame/assets/world/terrain/ground/dirt.png", 32, 32, 0, 0));

        registrar.RegisterPlaceable(new PlaceableDef(workbenchPlaceableId, "Workbench", PackId, "packs/basegame/assets/production/workstations/workbench.png", 28, 32, 0, 0, 3, true, "workbench", true));
        registrar.RegisterPlaceable(new PlaceableDef(torchPlaceableId, "Torch", PackId, "packs/basegame/assets/items/torch.png", 16, 16, 0, 0, 1));
        registrar.RegisterPlaceable(new PlaceableDef(chestPlaceableId, "Wooden Chest", PackId, "packs/basegame/assets/production/storage/wooden_chest.png", 32, 32, 0, 0, 3, false, null, true, true, 1, 0, true));
        registrar.RegisterPlaceable(new PlaceableDef(doorPlaceableId, "Wooden Door", PackId, "packs/basegame/assets/structures/doors/wood_door_close_open.png", 32, 32, 0, 0, 3, false, null, true, true, 1, 0, false));
        registrar.RegisterPlaceable(new PlaceableDef(fencePlaceableId, "Wooden Fence", PackId, "packs/basegame/assets/structures/fences/wood_fence_horizontal.png", 32, 32, 0, 0, 2, false, null, true));

        registrar.RegisterItem(new ItemDef(logItemId, "Log", PackId, "packs/basegame/assets/items/log_item.png", 28, 32, 99));
        registrar.RegisterItem(new ItemDef(stoneItemId, "Stone", PackId, "packs/basegame/assets/items/stone_item.png", 16, 16, 99));
        registrar.RegisterItem(new ItemDef(blueberryItemId, "Blueberry", PackId, "packs/basegame/assets/world/nature/plants/blueberry_bush.png", 16, 16, 20, null, 20));
        registrar.RegisterItem(new ItemDef(workbenchItemId, "Workbench", PackId, "packs/basegame/assets/production/workstations/workbench.png", 28, 32, 8, workbenchPlaceableId));
        registrar.RegisterItem(new ItemDef(torchItemId, "Torch", PackId, "packs/basegame/assets/items/torch.png", 16, 16, 16, torchPlaceableId));
        registrar.RegisterItem(new ItemDef(chestItemId, "Wooden Chest", PackId, "packs/basegame/assets/production/storage/wooden_chest.png", 32, 32, 8, chestPlaceableId));
        registrar.RegisterItem(new ItemDef(doorItemId, "Wooden Door", PackId, "packs/basegame/assets/structures/doors/wood_door_close_open.png", 32, 32, 8, doorPlaceableId));
        registrar.RegisterItem(new ItemDef(fenceItemId, "Wooden Fence", PackId, "packs/basegame/assets/structures/fences/wood_fence_horizontal.png", 32, 32, 32, fencePlaceableId));

        registrar.RegisterResourceNode(new ResourceNodeDef(
            treeNodeId,
            "Tree",
            PackId,
            "packs/basegame/assets/world/nature/trees/bright_green_tree.png",
            32,
            32,
            0,
            0,
            3,
            [new ItemAmount(logItemId, 3)]));

        registrar.RegisterResourceNode(new ResourceNodeDef(
            stoneNodeId,
            "Stone Node",
            PackId,
            "packs/basegame/assets/world/nature/rocks/stone.png",
            32,
            32,
            0,
            0,
            1,
            [new ItemAmount(stoneItemId, 1)],
            InstantPickup: true));

        registrar.RegisterResourceNode(new ResourceNodeDef(
            blueberryNodeId,
            "Blueberry Bush",
            PackId,
            "packs/basegame/assets/world/nature/plants/blueberry_bush.png",
            16,
            16,
            0,
            0,
            1,
            [new ItemAmount(blueberryItemId, 1)],
            InstantPickup: true));

        registrar.RegisterCreature(new CreatureDef(
            playerId,
            "Human",
            PackId,
            "packs/basegame/assets/characters/humans/default/idle.png",
            "packs/basegame/assets/characters/humans/default/run.png",
            null,
            64,
            64,
            140f));

        registrar.RegisterCreature(new CreatureDef(
            wormId,
            "Worm",
            PackId,
            "packs/basegame/assets/world/nature/plants/worm.png",
            "packs/basegame/assets/world/nature/plants/worm.png",
            "packs/basegame/assets/world/nature/plants/worm.png",
            16,
            16,
            28f,
            4,
            true,
            1));

        registrar.RegisterRecipe(new RecipeDef(
            new ContentId("basegame:craft_workbench"),
            "Workbench",
            PackId,
            [new ItemAmount(logItemId, 4), new ItemAmount(stoneItemId, 1)],
            new ItemAmount(workbenchItemId, 1)));

        registrar.RegisterRecipe(new RecipeDef(
            new ContentId("basegame:craft_torch"),
            "Torch",
            PackId,
            [new ItemAmount(logItemId, 1), new ItemAmount(stoneItemId, 1)],
            new ItemAmount(torchItemId, 1)));

        registrar.RegisterRecipe(new RecipeDef(
            new ContentId("basegame:craft_chest"),
            "Wooden Chest",
            PackId,
            [new ItemAmount(logItemId, 6)],
            new ItemAmount(chestItemId, 1),
            "workbench"));

        registrar.RegisterRecipe(new RecipeDef(
            new ContentId("basegame:craft_door"),
            "Wooden Door",
            PackId,
            [new ItemAmount(logItemId, 4)],
            new ItemAmount(doorItemId, 1),
            "workbench"));

        registrar.RegisterRecipe(new RecipeDef(
            new ContentId("basegame:craft_fence"),
            "Wooden Fence",
            PackId,
            [new ItemAmount(logItemId, 2)],
            new ItemAmount(fenceItemId, 2),
            "workbench"));

        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:fill-grass"), "fill-terrain", grassId));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:dirt-patch"), "dirt-patch", dirtId));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:spawn-trees"), "scatter-spawn", treeNodeId, 14, 1, 1, 20, 12));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:spawn-stones"), "scatter-spawn", stoneNodeId, 10, 2, 2, 20, 10));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:spawn-berries"), "scatter-spawn", blueberryNodeId, 8, 4, 1, 18, 12));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:spawn-worms"), "scatter-spawn", wormId, 3, 1, 1, 22, 12));

        registrar.SetBootstrapConfig(new GameBootstrapConfig(
            WorldWidth: 28,
            WorldHeight: 18,
            DefaultTerrainId: grassId,
            PlayerCreatureId: playerId,
            DebugItemId: stoneItemId,
            DebugPlaceableId: workbenchPlaceableId,
            DebugTerrainVariantId: dirtId,
            DayLengthSeconds: 90,
            StartingHealth: 100,
            StartingHunger: 100,
            MaxHealth: 100,
            MaxHunger: 100,
            PlayerSpawn: new TileSpawn(new TileCoord(10, 8)),
            DebugPlaceableSpawn: new TileSpawn(new TileCoord(12, 8))));
    }
}
