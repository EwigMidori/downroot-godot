using Downroot.Core.Definitions;
using Downroot.Core.Gameplay;
using Downroot.Core.World;

namespace Downroot.Core.Content;

public interface IContentRegistrar
{
    void RegisterItem(ItemDef itemDef);
    void RegisterPlaceable(PlaceableDef placeableDef);
    void RegisterRecipe(RecipeDef recipeDef);
    void RegisterCreature(CreatureDef creatureDef);
    void RegisterTerrain(TerrainDef terrainDef);
    void RegisterResourceNode(ResourceNodeDef resourceNodeDef);
    void RegisterRaisedFeature(RaisedFeatureDef raisedFeatureDef);
    void RegisterWorldGenPass(WorldGenPassDef passDef);
    void RegisterPortalWorldLink(PortalWorldLinkDef def);
    void RegisterRaisedOreFieldRule(RaisedOreFieldRuleDef def);
    void SetBootstrapConfig(GameBootstrapConfig config);
}
