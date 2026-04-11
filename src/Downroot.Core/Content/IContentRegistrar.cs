using Downroot.Core.Definitions;

namespace Downroot.Core.Content;

public interface IContentRegistrar
{
    void RegisterItem(ItemDef itemDef);
    void RegisterPlaceable(PlaceableDef placeableDef);
    void RegisterRecipe(RecipeDef recipeDef);
    void RegisterCreature(CreatureDef creatureDef);
    void RegisterTerrain(TerrainDef terrainDef);
}
