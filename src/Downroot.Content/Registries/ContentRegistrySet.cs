using Downroot.Core.Definitions;
using Downroot.Core.Gameplay;
using Downroot.Core.Registries;
using Downroot.Core.World;
using Downroot.Content.Registration;

namespace Downroot.Content.Registries;

public sealed class ContentRegistrySet
{
    public ContentRegistrySet()
    {
        Items = new Registry<ItemDef>();
        Placeables = new Registry<PlaceableDef>();
        Recipes = new Registry<RecipeDef>();
        Creatures = new Registry<CreatureDef>();
        Terrains = new Registry<TerrainDef>();
        ResourceNodes = new Registry<ResourceNodeDef>();
        RaisedFeatures = new Registry<RaisedFeatureDef>();
        WorldGenPasses = new List<WorldGenPassDef>();
        PortalWorldLinks = new List<PortalWorldLinkDef>();
        RaisedOreFieldRules = new List<RaisedOreFieldRuleDef>();
    }

    public IRegistry<ItemDef> Items { get; }
    public IRegistry<PlaceableDef> Placeables { get; }
    public IRegistry<RecipeDef> Recipes { get; }
    public IRegistry<CreatureDef> Creatures { get; }
    public IRegistry<TerrainDef> Terrains { get; }
    public IRegistry<ResourceNodeDef> ResourceNodes { get; }
    public IRegistry<RaisedFeatureDef> RaisedFeatures { get; }
    public IList<WorldGenPassDef> WorldGenPasses { get; }
    public IList<PortalWorldLinkDef> PortalWorldLinks { get; }
    public IList<RaisedOreFieldRuleDef> RaisedOreFieldRules { get; }
    public GameBootstrapConfig? BootstrapConfig { get; set; }

    public ContentRegistrar CreateRegistrar() => new(this);
}
