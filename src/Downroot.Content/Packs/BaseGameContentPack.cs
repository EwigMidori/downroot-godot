using Downroot.Core.Content;
using Downroot.Core.Definitions;
using Downroot.Core.Ids;

namespace Downroot.Content.Packs;

public sealed class BaseGameContentPack : IContentPack
{
    public const string Id = "basegame";

    public string PackId => Id;

    public void Register(IContentRegistrar registrar)
    {
        registrar.RegisterTerrain(new TerrainDef(
            new ContentId("basegame:grass"),
            "Grass",
            PackId,
            "packs/basegame/assets/world/terrain/ground/grass.png",
            32,
            32,
            0,
            0));

        registrar.RegisterTerrain(new TerrainDef(
            new ContentId("basegame:dirt"),
            "Dirt",
            PackId,
            "packs/basegame/assets/world/terrain/ground/dirt.png",
            32,
            32,
            0,
            0));

        registrar.RegisterItem(new ItemDef(
            new ContentId("basegame:stone"),
            "Stone",
            PackId,
            "packs/basegame/assets/world/nature/rocks/flat_stone.png"));

        registrar.RegisterPlaceable(new PlaceableDef(
            new ContentId("basegame:wooden_chest"),
            "Wooden Chest",
            PackId,
            "packs/basegame/assets/production/storage/wooden_chest.png"));

        registrar.RegisterCreature(new CreatureDef(
            new ContentId("basegame:player_human"),
            "Human",
            PackId,
            "packs/basegame/assets/characters/humans/default/idle.png",
            "packs/basegame/assets/characters/humans/default/run.png"));

        registrar.RegisterRecipe(new RecipeDef(
            new ContentId("basegame:debug_chest_recipe"),
            "Wooden Chest",
            PackId,
            ["basegame:stone"],
            "basegame:wooden_chest"));
    }
}
