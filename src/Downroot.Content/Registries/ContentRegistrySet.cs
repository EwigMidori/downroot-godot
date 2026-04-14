using Downroot.Core.Definitions;
using Downroot.Core.Gameplay;
using Downroot.Core.Registries;
using Downroot.Core.World;
using Downroot.Content.Registration;

namespace Downroot.Content.Registries;

public sealed class ContentRegistrySet
{
    private readonly List<WorldGenPassDef> _worldGenPasses;
    private readonly List<PortalWorldLinkDef> _portalWorldLinks;
    private readonly List<RaisedOreFieldRuleDef> _raisedOreFieldRules;

    public ContentRegistrySet()
    {
        Items = new Registry<ItemDef>();
        Placeables = new Registry<PlaceableDef>();
        Recipes = new Registry<RecipeDef>();
        Creatures = new Registry<CreatureDef>();
        Terrains = new Registry<TerrainDef>();
        ResourceNodes = new Registry<ResourceNodeDef>();
        RaisedFeatures = new Registry<RaisedFeatureDef>();
        _worldGenPasses = [];
        _portalWorldLinks = [];
        _raisedOreFieldRules = [];
    }

    public IRegistry<ItemDef> Items { get; }
    public IRegistry<PlaceableDef> Placeables { get; }
    public IRegistry<RecipeDef> Recipes { get; }
    public IRegistry<CreatureDef> Creatures { get; }
    public IRegistry<TerrainDef> Terrains { get; }
    public IRegistry<ResourceNodeDef> ResourceNodes { get; }
    public IRegistry<RaisedFeatureDef> RaisedFeatures { get; }
    public IReadOnlyList<WorldGenPassDef> WorldGenPasses => _worldGenPasses;
    public IReadOnlyList<PortalWorldLinkDef> PortalWorldLinks => _portalWorldLinks;
    public IReadOnlyList<RaisedOreFieldRuleDef> RaisedOreFieldRules => _raisedOreFieldRules;
    public GameBootstrapConfig? BootstrapConfig { get; set; }

    public ContentRegistrar CreateRegistrar() => new(this);

    internal void AddWorldGenPass(WorldGenPassDef passDef) => _worldGenPasses.Add(passDef);

    internal void AddPortalWorldLink(PortalWorldLinkDef linkDef) => _portalWorldLinks.Add(linkDef);

    internal void AddRaisedOreFieldRule(RaisedOreFieldRuleDef ruleDef) => _raisedOreFieldRules.Add(ruleDef);
}
