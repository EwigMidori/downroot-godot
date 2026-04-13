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
        var voiditeItemId = new ContentId("basegame:voidite");
        var goldveinItemId = new ContentId("basegame:goldvein");
        var venomiteItemId = new ContentId("basegame:venomite");
        var furnaceItemId = new ContentId("basegame:furnace_item");
        var voidCrystalItemId = new ContentId("basegame:void_crystal");
        var goldIngotItemId = new ContentId("basegame:gold_ingot");
        var poisonCrystalItemId = new ContentId("basegame:poison_crystal");
        var ironIngotItemId = new ContentId("basegame:iron_ingot");
        var sandItemId = new ContentId("basegame:sand");
        var siliconWaferItemId = new ContentId("basegame:silicon_wafer");
        var axeItemId = new ContentId("basegame:axe");
        var ironKnifeItemId = new ContentId("basegame:iron_knife");
        var stoneWallItemId = new ContentId("basegame:stone_wall_item");
        var stoneFloorItemId = new ContentId("basegame:stone_floor_item");
        var workbenchItemId = new ContentId("basegame:workbench_item");
        var torchItemId = new ContentId("basegame:torch");
        var chestItemId = new ContentId("basegame:wooden_chest_item");
        var doorItemId = new ContentId("basegame:wooden_door_item");
        var fenceItemId = new ContentId("basegame:wooden_fence_item");

        var furnacePlaceableId = new ContentId("basegame:furnace");
        var stoneWallPlaceableId = new ContentId("basegame:stone_wall");
        var stoneFloorPlaceableId = new ContentId("basegame:stone_floor");
        var workbenchPlaceableId = new ContentId("basegame:workbench");
        var torchPlaceableId = new ContentId("basegame:torch_placeable");
        var chestPlaceableId = new ContentId("basegame:wooden_chest");
        var doorPlaceableId = new ContentId("basegame:wooden_door");
        var fencePlaceableId = new ContentId("basegame:wooden_fence");

        var playerId = new ContentId("basegame:player_human");
        var wormId = new ContentId("basegame:worm");
        var cockroachId = new ContentId("basegame:cockroach");

        var treeNodeId = new ContentId("basegame:tree_bright");
        var stoneNodeId = new ContentId("basegame:stone_node");
        var blueberryNodeId = new ContentId("basegame:blueberry_bush");
        var voiditeNodeId = new ContentId("basegame:voidite_node");
        var goldveinNodeId = new ContentId("basegame:goldvein_node");
        var venomiteNodeId = new ContentId("basegame:venomite_node");

        registrar.RegisterTerrain(new TerrainDef(grassId, "Grass", PackId, "packs/basegame/assets/world/terrain/ground/grass.png", 32, 32, 0, 0));
        registrar.RegisterTerrain(new TerrainDef(dirtId, "Dirt", PackId, "packs/basegame/assets/world/terrain/ground/dirt.png", 32, 32, 0, 0));

        registrar.RegisterPlaceable(new PlaceableDef(furnacePlaceableId, "Furnace", PackId, "packs/basegame/assets/production/utility/furnace.png", 32, 32, 0, 0, 5, true, "furnace", true));
        registrar.RegisterPlaceable(new PlaceableDef(stoneWallPlaceableId, "Stone Wall", PackId, "packs/basegame/assets/structures/walls/stone_wall.png", 32, 32, 0, 0, 5, false, null, true));
        registrar.RegisterPlaceable(new PlaceableDef(stoneFloorPlaceableId, "Stone Floor", PackId, "packs/basegame/assets/world/terrain/floors/stone_floor.png", 32, 32, 0, 0, 2, false, null, false));
        registrar.RegisterPlaceable(new PlaceableDef(workbenchPlaceableId, "Workbench", PackId, "packs/basegame/assets/production/workstations/workbench.png", 28, 32, 0, 0, 3, true, "workbench", true));
        registrar.RegisterPlaceable(new PlaceableDef(torchPlaceableId, "Torch", PackId, "packs/basegame/assets/items/torch.png", 16, 16, 0, 0, 1));
        registrar.RegisterPlaceable(new PlaceableDef(chestPlaceableId, "Wooden Chest", PackId, "packs/basegame/assets/production/storage/wooden_chest.png", 32, 32, 0, 0, 3, false, null, true, true, 1, 0, true));
        registrar.RegisterPlaceable(new PlaceableDef(doorPlaceableId, "Wooden Door", PackId, "packs/basegame/assets/structures/doors/wood_door_close_open.png", 32, 32, 0, 0, 3, false, null, true, true, 1, 0, false));
        registrar.RegisterPlaceable(new PlaceableDef(fencePlaceableId, "Wooden Fence", PackId, "packs/basegame/assets/structures/fences/wood_fence_horizontal.png", 32, 32, 0, 0, 2, false, null, true));

        registrar.RegisterItem(new ItemDef(logItemId, "Log", PackId, "packs/basegame/assets/items/log_item.png", 28, 32, 99));
        registrar.RegisterItem(new ItemDef(stoneItemId, "Stone", PackId, "packs/basegame/assets/items/stone_item.png", 16, 16, 99));
        registrar.RegisterItem(new ItemDef(blueberryItemId, "Blueberry", PackId, "packs/basegame/assets/world/nature/plants/blueberry_bush.png", 16, 16, 20, null, 20));
        registrar.RegisterItem(new ItemDef(voiditeItemId, "Voidite", PackId, "packs/basegame/assets/world/nature/ores/voidite.png", 32, 32, 32));
        registrar.RegisterItem(new ItemDef(goldveinItemId, "Goldvein", PackId, "packs/basegame/assets/world/nature/ores/goldvein.png", 32, 32, 32));
        registrar.RegisterItem(new ItemDef(venomiteItemId, "Venomite", PackId, "packs/basegame/assets/world/nature/ores/venomite.png", 32, 32, 32));
        registrar.RegisterItem(new ItemDef(furnaceItemId, "Furnace", PackId, "packs/basegame/assets/items/resources/furnace_item.png", 16, 16, 8, furnacePlaceableId));
        registrar.RegisterItem(new ItemDef(voidCrystalItemId, "Void Crystal", PackId, "packs/basegame/assets/items/resources/void_crystal.png", 16, 16, 32));
        registrar.RegisterItem(new ItemDef(goldIngotItemId, "Gold Ingot", PackId, "packs/basegame/assets/items/resources/gold_ingot.png", 16, 16, 32));
        registrar.RegisterItem(new ItemDef(poisonCrystalItemId, "Poison Crystal", PackId, "packs/basegame/assets/items/resources/poison_crystal.png", 16, 16, 32));
        registrar.RegisterItem(new ItemDef(ironIngotItemId, "Iron Ingot", PackId, "packs/basegame/assets/items/resources/iron_ingot.png", 16, 16, 32));
        registrar.RegisterItem(new ItemDef(sandItemId, "Sand", PackId, "packs/basegame/assets/items/resources/sand.png", 16, 16, 99));
        registrar.RegisterItem(new ItemDef(siliconWaferItemId, "Silicon Wafer", PackId, "packs/basegame/assets/items/resources/silicon_wafer.png", 16, 16, 32));
        registrar.RegisterItem(new ItemDef(axeItemId, "Axe", PackId, "packs/basegame/assets/items/tools/axe.png", 16, 16, 1, null, 0, 0, 2f));
        registrar.RegisterItem(new ItemDef(ironKnifeItemId, "Iron Knife", PackId, "packs/basegame/assets/items/weapons/iron_knife.png", 16, 16, 1, null, 0, 0, 1f, 3));
        registrar.RegisterItem(new ItemDef(stoneWallItemId, "Stone Wall", PackId, "packs/basegame/assets/structures/walls/stone_wall.png", 32, 32, 32, stoneWallPlaceableId));
        registrar.RegisterItem(new ItemDef(stoneFloorItemId, "Stone Floor", PackId, "packs/basegame/assets/world/terrain/floors/stone_floor.png", 32, 32, 64, stoneFloorPlaceableId));
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
            [new ItemAmount(logItemId, 3)],
            IsTree: true));

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

        registrar.RegisterResourceNode(new ResourceNodeDef(
            voiditeNodeId,
            "Voidite",
            PackId,
            "packs/basegame/assets/world/nature/ores/voidite.png",
            32,
            32,
            0,
            0,
            4,
            [new ItemAmount(voiditeItemId, 1)]));

        registrar.RegisterResourceNode(new ResourceNodeDef(
            goldveinNodeId,
            "Goldvein",
            PackId,
            "packs/basegame/assets/world/nature/ores/goldvein.png",
            32,
            32,
            0,
            0,
            4,
            [new ItemAmount(goldveinItemId, 1)]));

        registrar.RegisterResourceNode(new ResourceNodeDef(
            venomiteNodeId,
            "Venomite",
            PackId,
            "packs/basegame/assets/world/nature/ores/venomite.png",
            32,
            32,
            0,
            0,
            4,
            [new ItemAmount(venomiteItemId, 1)]));

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

        registrar.RegisterCreature(new CreatureDef(
            cockroachId,
            "Cockroach",
            PackId,
            "packs/basegame/assets/world/nature/plants/cockroach.png",
            "packs/basegame/assets/world/nature/plants/cockroach.png",
            "packs/basegame/assets/world/nature/plants/cockroach.png",
            16,
            16,
            34f,
            1,
            false,
            2,
            128f,
            192f,
            160f,
            1f));

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

        registrar.RegisterRecipe(new RecipeDef(
            new ContentId("basegame:craft_furnace"),
            "Furnace",
            PackId,
            [new ItemAmount(stoneItemId, 4), new ItemAmount(ironIngotItemId, 1)],
            new ItemAmount(furnaceItemId, 1),
            "workbench"));

        registrar.RegisterRecipe(new RecipeDef(
            new ContentId("basegame:craft_stone_wall"),
            "Stone Wall",
            PackId,
            [new ItemAmount(stoneItemId, 2)],
            new ItemAmount(stoneWallItemId, 1),
            "workbench"));

        registrar.RegisterRecipe(new RecipeDef(
            new ContentId("basegame:craft_stone_floor"),
            "Stone Floor",
            PackId,
            [new ItemAmount(stoneItemId, 1)],
            new ItemAmount(stoneFloorItemId, 1),
            "workbench"));

        registrar.RegisterRecipe(new RecipeDef(
            new ContentId("basegame:craft_axe"),
            "Axe",
            PackId,
            [new ItemAmount(logItemId, 1), new ItemAmount(ironIngotItemId, 1)],
            new ItemAmount(axeItemId, 1),
            "workbench"));

        registrar.RegisterRecipe(new RecipeDef(
            new ContentId("basegame:craft_iron_knife"),
            "Iron Knife",
            PackId,
            [new ItemAmount(logItemId, 1), new ItemAmount(ironIngotItemId, 2)],
            new ItemAmount(ironKnifeItemId, 1),
            "workbench"));

        registrar.RegisterRecipe(new RecipeDef(
            new ContentId("basegame:smelt_voidite"),
            "Void Crystal",
            PackId,
            [new ItemAmount(voiditeItemId, 1)],
            new ItemAmount(voidCrystalItemId, 2),
            "furnace",
            3f));

        registrar.RegisterRecipe(new RecipeDef(
            new ContentId("basegame:smelt_goldvein"),
            "Gold Ingot + Sand",
            PackId,
            [new ItemAmount(goldveinItemId, 1)],
            new ItemAmount(goldIngotItemId, 1),
            "furnace",
            3.5f,
            [new ItemAmount(sandItemId, 1)]));

        registrar.RegisterRecipe(new RecipeDef(
            new ContentId("basegame:smelt_venomite"),
            "Poison Crystal + Iron Ingot",
            PackId,
            [new ItemAmount(venomiteItemId, 1)],
            new ItemAmount(poisonCrystalItemId, 1),
            "furnace",
            3.5f,
            [new ItemAmount(ironIngotItemId, 1)]));

        registrar.RegisterRecipe(new RecipeDef(
            new ContentId("basegame:smelt_silicon_wafer"),
            "Silicon Wafer",
            PackId,
            [new ItemAmount(sandItemId, 8)],
            new ItemAmount(siliconWaferItemId, 1),
            "furnace",
            5f));

        registrar.RegisterWorldGenPass(new WorldGenPassDef(
            new ContentId("basegame:fill-dirt"),
            "fill-terrain",
            dirtId,
            PrimarySurfaceRegion: SurfaceRegions.DirtField));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:grass-region"), "grass-region", grassId));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:spawn-trees"), "scatter-spawn", treeNodeId, 14, 0, 0, 28, 18, SurfaceRegions.GrassField, 3));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:spawn-stones"), "scatter-spawn", stoneNodeId, 10, 0, 0, 28, 18, SurfaceRegions.DirtField, 2));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:spawn-berries"), "scatter-spawn", blueberryNodeId, 8, 0, 0, 28, 18, SurfaceRegions.GrassField, 2));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:spawn-voidite"), "scatter-spawn", voiditeNodeId, 4, 0, 0, 28, 18, SurfaceRegions.DirtField, 4));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:spawn-goldvein"), "scatter-spawn", goldveinNodeId, 4, 0, 0, 28, 18, SurfaceRegions.DirtField, 4));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:spawn-venomite"), "scatter-spawn", venomiteNodeId, 4, 0, 0, 28, 18, SurfaceRegions.DirtField, 4));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:spawn-worms"), "scatter-spawn", wormId, 3, 0, 0, 28, 18, SurfaceRegions.DirtField, 5));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:spawn-cockroaches"), "scatter-spawn", cockroachId, 4, 0, 0, 28, 18, SurfaceRegions.GrassField, 5));

        registrar.SetBootstrapConfig(new GameBootstrapConfig(
            WorldWidth: 28,
            WorldHeight: 18,
            DefaultTerrainId: dirtId,
            PlayerCreatureId: playerId,
            DebugItemId: stoneItemId,
            DebugPlaceableId: workbenchPlaceableId,
            DebugTerrainVariantId: grassId,
            DayLengthSeconds: 90,
            StartingHealth: 100,
            StartingHunger: 100,
            MaxHealth: 100,
            MaxHunger: 100,
            PlayerSpawn: new TileSpawn(new TileCoord(10, 8)),
            DebugPlaceableSpawn: new TileSpawn(new TileCoord(12, 8))));
    }
}
