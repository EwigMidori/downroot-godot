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
        var stoneId = new ContentId("basegame:stone_debug");
        var chestId = new ContentId("basegame:wooden_chest");
        var playerId = new ContentId("basegame:player_human");

        registrar.RegisterTerrain(new TerrainDef(
            grassId,
            "Grass",
            PackId,
            "packs/basegame/assets/world/terrain/ground/grass.png",
            32,
            32,
            0,
            0));

        registrar.RegisterTerrain(new TerrainDef(
            dirtId,
            "Dirt",
            PackId,
            "packs/basegame/assets/world/terrain/ground/dirt.png",
            32,
            32,
            0,
            0));

        registrar.RegisterItem(new ItemDef(
            stoneId,
            "Stone Debug Pickup",
            PackId,
            // Phase 0 placeholder: this project does not yet ship item-specific icons.
            "packs/basegame/assets/world/nature/rocks/flat_stone.png"));

        registrar.RegisterPlaceable(new PlaceableDef(
            chestId,
            "Wooden Chest",
            PackId,
            "packs/basegame/assets/production/storage/wooden_chest.png"));

        registrar.RegisterCreature(new CreatureDef(
            playerId,
            "Human",
            PackId,
            "packs/basegame/assets/characters/humans/default/idle.png",
            "packs/basegame/assets/characters/humans/default/run.png"));

        registrar.RegisterRecipe(new RecipeDef(
            new ContentId("basegame:debug_chest_recipe"),
            "Wooden Chest",
            PackId,
            [stoneId.Value],
            chestId.Value));

        registrar.RegisterWorldGenPass(new WorldGenPassDef(
            "basegame:fill-grass",
            "fill-terrain",
            grassId.Value));

        registrar.RegisterWorldGenPass(new WorldGenPassDef(
            "basegame:dirt-patch",
            "dirt-patch",
            dirtId.Value));

        registrar.SetBootstrapConfig(new GameBootstrapConfig(
            WorldWidth: 24,
            WorldHeight: 16,
            DefaultTerrainId: grassId,
            PlayerCreatureId: playerId,
            DebugItemId: stoneId,
            DebugPlaceableId: chestId,
            DebugTerrainVariantId: dirtId,
            PlayerSpawn: new TileSpawn(new TileCoord(12, 8)),
            DebugPlaceableSpawn: new TileSpawn(new TileCoord(5, 5))));
    }
}
