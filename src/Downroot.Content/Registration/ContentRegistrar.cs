using Downroot.Core.Content;
using Downroot.Core.Definitions;
using Downroot.Core.Gameplay;
using Downroot.Core.World;
using Downroot.Content.Registries;

namespace Downroot.Content.Registration;

public sealed class ContentRegistrar(ContentRegistrySet registries) : IContentRegistrar
{
    public void RegisterItem(ItemDef itemDef) => registries.Items.Register(itemDef);

    public void RegisterPlaceable(PlaceableDef placeableDef) => registries.Placeables.Register(placeableDef);

    public void RegisterRecipe(RecipeDef recipeDef) => registries.Recipes.Register(recipeDef);

    public void RegisterCreature(CreatureDef creatureDef) => registries.Creatures.Register(creatureDef);

    public void RegisterTerrain(TerrainDef terrainDef) => registries.Terrains.Register(terrainDef);

    public void RegisterResourceNode(ResourceNodeDef resourceNodeDef) => registries.ResourceNodes.Register(resourceNodeDef);

    public void RegisterRaisedFeature(RaisedFeatureDef raisedFeatureDef) => registries.RaisedFeatures.Register(raisedFeatureDef);

    public void RegisterWorldGenPass(WorldGenPassDef passDef) => registries.WorldGenPasses.Add(passDef);

    public void SetBootstrapConfig(GameBootstrapConfig config) => registries.BootstrapConfig = config;
}
