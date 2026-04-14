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
        var riverWaterId = new ContentId("basegame:river_water");
        var dimfragId = new ContentId("basegame:dimfrag");

        var logItemId = new ContentId("basegame:log");
        var stoneItemId = new ContentId("basegame:stone");
        var blueberryItemId = new ContentId("basegame:blueberry");
        var voiditeItemId = new ContentId("basegame:voidite");
        var goldveinItemId = new ContentId("basegame:goldvein");
        var venomiteItemId = new ContentId("basegame:venomite");
        var frostcoreItemId = new ContentId("basegame:frostcore");
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
        var portalPlaceableId = new ContentId("basegame:portal");

        var playerId = new ContentId("basegame:player_human");
        var wormId = new ContentId("basegame:worm");
        var cockroachId = new ContentId("basegame:cockroach");

        var treeNodeId = new ContentId("basegame:tree_bright");
        var stoneNodeId = new ContentId("basegame:stone_node");
        var blueberryNodeId = new ContentId("basegame:blueberry_bush");
        var rockOutcropNodeId = new ContentId("basegame:rock_outcrop");
        var voiditeRaisedId = new ContentId("basegame:voidite_raised");
        var goldveinRaisedId = new ContentId("basegame:goldvein_raised");
        var venomiteRaisedId = new ContentId("basegame:venomite_raised");
        var frostcoreRaisedId = new ContentId("basegame:frostcore_raised");
        var portalLink = new PortalWorldLinkDef(
            WorldSpaceKind.Overworld,
            WorldSpaceKind.DimShardPocket,
            new ChunkCoord(1, 0),
            new ChunkCoord(0, 0),
            "overworld-1,0-to-dimshard-0,0");

        registrar.RegisterTerrain(new TerrainDef(grassId, "Grass", PackId, "packs/basegame/assets/world/terrain/ground/grass.png", 32, 32, 0, 0));
        registrar.RegisterTerrain(new TerrainDef(dirtId, "Dirt", PackId, "packs/basegame/assets/world/terrain/ground/dirt.png", 32, 32, 0, 0));
        registrar.RegisterTerrain(new TerrainDef(riverWaterId, "River Water", PackId, "packs/basegame/assets/world/terrain/ground/river_water.png", 32, 32, 0, 0));
        registrar.RegisterTerrain(new TerrainDef(dimfragId, "Dimfrag", PackId, "packs/basegame/assets/world/terrain/ground/dimfrag.png", 32, 32, 0, 0));

        registrar.RegisterPlaceable(new PlaceableDef(furnacePlaceableId, "Furnace", PackId, "packs/basegame/assets/production/utility/furnace.png", 32, 32, 0, 0, 5, true, CraftingStationKind.Furnace, true));
        registrar.RegisterPlaceable(new PlaceableDef(stoneWallPlaceableId, "Stone Wall", PackId, "packs/basegame/assets/structures/walls/stone_wall.png", 32, 32, 0, 0, 5, false, null, true));
        registrar.RegisterPlaceable(new PlaceableDef(stoneFloorPlaceableId, "Stone Floor", PackId, "packs/basegame/assets/world/terrain/floors/stone_floor.png", 32, 32, 0, 0, 2, false, null, false, false, 0, 0, false, true));
        registrar.RegisterPlaceable(new PlaceableDef(workbenchPlaceableId, "Workbench", PackId, "packs/basegame/assets/production/workstations/workbench.png", 28, 32, 0, 0, 3, true, CraftingStationKind.Workbench, true));
        registrar.RegisterPlaceable(new PlaceableDef(torchPlaceableId, "Torch", PackId, "packs/basegame/assets/items/torch.png", 16, 16, 0, 0, 1));
        registrar.RegisterPlaceable(new PlaceableDef(chestPlaceableId, "Wooden Chest", PackId, "packs/basegame/assets/production/storage/wooden_chest.png", 32, 32, 0, 0, 3, false, null, true, true, 1, 0, true));
        registrar.RegisterPlaceable(new PlaceableDef(doorPlaceableId, "Wooden Door", PackId, "packs/basegame/assets/structures/doors/wood_door_close_open.png", 32, 32, 0, 0, 3, false, null, true, true, 1, 0, false));
        registrar.RegisterPlaceable(new PlaceableDef(fencePlaceableId, "Wooden Fence", PackId, "packs/basegame/assets/structures/fences/wood_fence_horizontal.png", 32, 32, 0, 0, 2, false, null, true));
        registrar.RegisterPlaceable(new PlaceableDef(portalPlaceableId, "Portal", PackId, "packs/basegame/assets/world/nature/ruins/portal.png", 32, 32, 0, 0, 999, false, null, false, false, 0, 0, false, false, false));

        registrar.RegisterItem(new ItemDef(logItemId, "Log", PackId, "packs/basegame/assets/items/log_item.png", 28, 32, 99));
        registrar.RegisterItem(new ItemDef(stoneItemId, "Stone", PackId, "packs/basegame/assets/items/stone_item.png", 16, 16, 99));
        registrar.RegisterItem(new ItemDef(blueberryItemId, "Blueberry", PackId, "packs/basegame/assets/world/nature/plants/blueberry_bush.png", 16, 16, 20, null, 20));
        registrar.RegisterItem(new ItemDef(voiditeItemId, "Voidite", PackId, "packs/basegame/assets/world/nature/ores/voidite.png", 32, 32, 32));
        registrar.RegisterItem(new ItemDef(goldveinItemId, "Goldvein", PackId, "packs/basegame/assets/world/nature/ores/goldvein.png", 32, 32, 32));
        registrar.RegisterItem(new ItemDef(venomiteItemId, "Venomite", PackId, "packs/basegame/assets/world/nature/ores/venomite.png", 32, 32, 32));
        registrar.RegisterItem(new ItemDef(frostcoreItemId, "Frostcore", PackId, "packs/basegame/assets/world/nature/ores/frostcore.png", 32, 32, 32));
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

        registrar.RegisterResourceNode(new ResourceNodeDef(treeNodeId, "Tree", PackId, "packs/basegame/assets/world/nature/trees/bright_green_tree.png", 32, 32, 0, 0, 3, [new ItemAmount(logItemId, 3)], true, false, false, 0, true));
        registrar.RegisterResourceNode(new ResourceNodeDef(stoneNodeId, "Stone Node", PackId, "packs/basegame/assets/world/nature/rocks/stone.png", 32, 32, 0, 0, 1, [new ItemAmount(stoneItemId, 1)], false, true));
        registrar.RegisterResourceNode(new ResourceNodeDef(blueberryNodeId, "Blueberry Bush", PackId, "packs/basegame/assets/world/nature/plants/blueberry_bush.png", 16, 16, 0, 0, 1, [new ItemAmount(blueberryItemId, 1)], false, true));
        registrar.RegisterResourceNode(new ResourceNodeDef(rockOutcropNodeId, "Rock Outcrop", PackId, "packs/basegame/assets/world/nature/rocks/rock_outcrop.png", 32, 32, 0, 0, 4, [new ItemAmount(stoneItemId, 2)], true));
        registrar.RegisterRaisedFeature(new RaisedFeatureDef(voiditeRaisedId, "Voidite", PackId, "packs/basegame/assets/world/nature/ores/voidite.png", 32, 32, 13, 4, [new ItemAmount(voiditeItemId, 1)]));
        registrar.RegisterRaisedFeature(new RaisedFeatureDef(goldveinRaisedId, "Goldvein", PackId, "packs/basegame/assets/world/nature/ores/goldvein.png", 32, 32, 13, 4, [new ItemAmount(goldveinItemId, 1)]));
        registrar.RegisterRaisedFeature(new RaisedFeatureDef(venomiteRaisedId, "Venomite", PackId, "packs/basegame/assets/world/nature/ores/venomite.png", 32, 32, 13, 4, [new ItemAmount(venomiteItemId, 1)]));
        registrar.RegisterRaisedFeature(new RaisedFeatureDef(frostcoreRaisedId, "Frostcore", PackId, "packs/basegame/assets/world/nature/ores/frostcore.png", 32, 32, 13, 5, [new ItemAmount(frostcoreItemId, 1)]));
        registrar.RegisterPortalWorldLink(portalLink);
        registrar.RegisterRaisedOreFieldRule(new RaisedOreFieldRuleDef(voiditeRaisedId, WorldSpaceKind.Overworld, SurfaceRegions.DirtField, 0.48f, true, [voiditeRaisedId, goldveinRaisedId, venomiteRaisedId]));
        registrar.RegisterRaisedOreFieldRule(new RaisedOreFieldRuleDef(goldveinRaisedId, WorldSpaceKind.Overworld, SurfaceRegions.DirtField, 0.48f, true, [voiditeRaisedId, goldveinRaisedId, venomiteRaisedId]));
        registrar.RegisterRaisedOreFieldRule(new RaisedOreFieldRuleDef(venomiteRaisedId, WorldSpaceKind.Overworld, SurfaceRegions.DirtField, 0.48f, true, [voiditeRaisedId, goldveinRaisedId, venomiteRaisedId]));
        registrar.RegisterRaisedOreFieldRule(new RaisedOreFieldRuleDef(frostcoreRaisedId, WorldSpaceKind.DimShardPocket, SurfaceRegions.DimShardField, 0.24f, false, [frostcoreRaisedId]));

        registrar.RegisterCreature(new CreatureDef(playerId, "Human", PackId, "packs/basegame/assets/characters/humans/default/idle.png", "packs/basegame/assets/characters/humans/default/run.png", null, 64, 64, 140f));
        registrar.RegisterCreature(new CreatureDef(wormId, "Worm", PackId, "packs/basegame/assets/world/nature/plants/worm.png", "packs/basegame/assets/world/nature/plants/worm.png", "packs/basegame/assets/world/nature/plants/worm.png", 16, 16, 28f, 4, true, 1));
        registrar.RegisterCreature(new CreatureDef(cockroachId, "Cockroach", PackId, "packs/basegame/assets/world/nature/plants/cockroach.png", "packs/basegame/assets/world/nature/plants/cockroach.png", "packs/basegame/assets/world/nature/plants/cockroach.png", 16, 16, 34f, 1, false, 2, 128f, 192f, 160f, 1f));

        registrar.RegisterRecipe(new RecipeDef(new ContentId("basegame:craft_workbench"), "Workbench", PackId, [new ItemAmount(logItemId, 4), new ItemAmount(stoneItemId, 1)], new ItemAmount(workbenchItemId, 1), CraftingStationKind.Handcraft));
        registrar.RegisterRecipe(new RecipeDef(new ContentId("basegame:craft_torch"), "Torch", PackId, [new ItemAmount(logItemId, 1), new ItemAmount(stoneItemId, 1)], new ItemAmount(torchItemId, 1), CraftingStationKind.Handcraft));
        registrar.RegisterRecipe(new RecipeDef(new ContentId("basegame:craft_chest"), "Wooden Chest", PackId, [new ItemAmount(logItemId, 6)], new ItemAmount(chestItemId, 1), CraftingStationKind.Workbench));
        registrar.RegisterRecipe(new RecipeDef(new ContentId("basegame:craft_door"), "Wooden Door", PackId, [new ItemAmount(logItemId, 4)], new ItemAmount(doorItemId, 1), CraftingStationKind.Workbench));
        registrar.RegisterRecipe(new RecipeDef(new ContentId("basegame:craft_fence"), "Wooden Fence", PackId, [new ItemAmount(logItemId, 2)], new ItemAmount(fenceItemId, 2), CraftingStationKind.Workbench));
        registrar.RegisterRecipe(new RecipeDef(new ContentId("basegame:craft_furnace"), "Furnace", PackId, [new ItemAmount(stoneItemId, 4)], new ItemAmount(furnaceItemId, 1), CraftingStationKind.Workbench));
        registrar.RegisterRecipe(new RecipeDef(new ContentId("basegame:craft_stone_wall"), "Stone Wall", PackId, [new ItemAmount(stoneItemId, 2)], new ItemAmount(stoneWallItemId, 1), CraftingStationKind.Workbench));
        registrar.RegisterRecipe(new RecipeDef(new ContentId("basegame:craft_stone_floor"), "Stone Floor", PackId, [new ItemAmount(stoneItemId, 1)], new ItemAmount(stoneFloorItemId, 1), CraftingStationKind.Workbench));
        registrar.RegisterRecipe(new RecipeDef(new ContentId("basegame:craft_axe"), "Axe", PackId, [new ItemAmount(logItemId, 1), new ItemAmount(ironIngotItemId, 1)], new ItemAmount(axeItemId, 1), CraftingStationKind.Workbench));
        registrar.RegisterRecipe(new RecipeDef(new ContentId("basegame:craft_iron_knife"), "Iron Knife", PackId, [new ItemAmount(logItemId, 1), new ItemAmount(ironIngotItemId, 2)], new ItemAmount(ironKnifeItemId, 1), CraftingStationKind.Workbench));
        registrar.RegisterRecipe(new RecipeDef(new ContentId("basegame:smelt_voidite"), "Void Crystal", PackId, [new ItemAmount(voiditeItemId, 1)], new ItemAmount(voidCrystalItemId, 2), CraftingStationKind.Furnace, 3f));
        registrar.RegisterRecipe(new RecipeDef(new ContentId("basegame:smelt_goldvein"), "Gold Ingot + Sand", PackId, [new ItemAmount(goldveinItemId, 1)], new ItemAmount(goldIngotItemId, 1), CraftingStationKind.Furnace, 3.5f, [new ItemAmount(sandItemId, 1)]));
        registrar.RegisterRecipe(new RecipeDef(new ContentId("basegame:smelt_venomite"), "Poison Crystal + Iron Ingot", PackId, [new ItemAmount(venomiteItemId, 1)], new ItemAmount(poisonCrystalItemId, 1), CraftingStationKind.Furnace, 3.5f, [new ItemAmount(ironIngotItemId, 1)]));
        registrar.RegisterRecipe(new RecipeDef(new ContentId("basegame:smelt_silicon_wafer"), "Silicon Wafer", PackId, [new ItemAmount(sandItemId, 8)], new ItemAmount(siliconWaferItemId, 1), CraftingStationKind.Furnace, 5f));

        RegisterOverworldPasses(registrar, dirtId, grassId, riverWaterId, rockOutcropNodeId, treeNodeId, blueberryNodeId, stoneNodeId, voiditeRaisedId, goldveinRaisedId, venomiteRaisedId, wormId, cockroachId, portalPlaceableId);
        RegisterPocketWorldPasses(registrar, dimfragId, rockOutcropNodeId, frostcoreRaisedId, portalPlaceableId);

        registrar.SetBootstrapConfig(new GameBootstrapConfig(
            ChunkWidth: 28,
            ChunkHeight: 18,
            WorldSeed: 424242,
            OverworldLoadRadius: 1,
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
            PlayerSpawn: new TileSpawn(new WorldTileCoord(10, 8)),
            DebugPlaceableSpawn: new TileSpawn(new WorldTileCoord(12, 8))));
    }

    private static void RegisterOverworldPasses(
        IContentRegistrar registrar,
        ContentId dirtId,
        ContentId grassId,
        ContentId riverWaterId,
        ContentId rockOutcropNodeId,
        ContentId treeNodeId,
        ContentId blueberryNodeId,
        ContentId stoneNodeId,
        ContentId voiditeRaisedId,
        ContentId goldveinRaisedId,
        ContentId venomiteRaisedId,
        ContentId wormId,
        ContentId cockroachId,
        ContentId portalPlaceableId)
    {
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:overworld-fill-dirt"), WorldGenPassTypes.FillTerrain, dirtId, WorldSpaceKind.Overworld, PrimarySurfaceRegion: SurfaceRegions.DirtField));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:overworld-surface-region"), WorldGenPassTypes.SurfaceRegion, grassId, WorldSpaceKind.Overworld));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:overworld-river"), WorldGenPassTypes.River, riverWaterId, WorldSpaceKind.Overworld));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:raised-voidite"), WorldGenPassTypes.RaisedOreField, voiditeRaisedId, WorldSpaceKind.Overworld));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:raised-goldvein"), WorldGenPassTypes.RaisedOreField, goldveinRaisedId, WorldSpaceKind.Overworld));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:raised-venomite"), WorldGenPassTypes.RaisedOreField, venomiteRaisedId, WorldSpaceKind.Overworld));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:overworld-rock-outcrop"), WorldGenPassTypes.RockOutcrop, rockOutcropNodeId, WorldSpaceKind.Overworld));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:spawn-trees"), WorldGenPassTypes.ScatterSpawn, treeNodeId, WorldSpaceKind.Overworld, 14, 0, 0, 28, 18, SurfaceRegions.GrassField, 3));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:spawn-berries"), WorldGenPassTypes.ScatterSpawn, blueberryNodeId, WorldSpaceKind.Overworld, 8, 0, 0, 28, 18, SurfaceRegions.GrassField, 2));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:spawn-stones"), WorldGenPassTypes.ScatterSpawn, stoneNodeId, WorldSpaceKind.Overworld, 10, 0, 0, 28, 18, SurfaceRegions.DirtField, 2));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:spawn-worms"), WorldGenPassTypes.ScatterSpawn, wormId, WorldSpaceKind.Overworld, 3, 0, 0, 28, 18, SurfaceRegions.DirtField, 5));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:spawn-cockroaches"), WorldGenPassTypes.ScatterSpawn, cockroachId, WorldSpaceKind.Overworld, 4, 0, 0, 28, 18, SurfaceRegions.GrassField, 5));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:portal-site"), WorldGenPassTypes.PortalSite, portalPlaceableId, WorldSpaceKind.Overworld));
    }

    private static void RegisterPocketWorldPasses(
        IContentRegistrar registrar,
        ContentId dimfragId,
        ContentId rockOutcropNodeId,
        ContentId frostcoreRaisedId,
        ContentId portalPlaceableId)
    {
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:dim-fill"), WorldGenPassTypes.FillTerrain, dimfragId, WorldSpaceKind.DimShardPocket, PrimarySurfaceRegion: SurfaceRegions.DimShardField));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:dim-raised-frostcore"), WorldGenPassTypes.RaisedOreField, frostcoreRaisedId, WorldSpaceKind.DimShardPocket));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:dim-rock-outcrop"), WorldGenPassTypes.RockOutcrop, rockOutcropNodeId, WorldSpaceKind.DimShardPocket));
        registrar.RegisterWorldGenPass(new WorldGenPassDef(new ContentId("basegame:dim-portal"), WorldGenPassTypes.PortalSite, portalPlaceableId, WorldSpaceKind.DimShardPocket));
    }
}
