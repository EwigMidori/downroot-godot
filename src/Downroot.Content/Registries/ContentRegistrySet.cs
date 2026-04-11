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
        WorldGenPasses = new List<WorldGenPassDef>();
    }

    public IRegistry<ItemDef> Items { get; }
    public IRegistry<PlaceableDef> Placeables { get; }
    public IRegistry<RecipeDef> Recipes { get; }
    public IRegistry<CreatureDef> Creatures { get; }
    public IRegistry<TerrainDef> Terrains { get; }
    public IRegistry<ResourceNodeDef> ResourceNodes { get; }
    public IList<WorldGenPassDef> WorldGenPasses { get; }
    public GameBootstrapConfig? BootstrapConfig { get; set; }

    public ContentRegistrar CreateRegistrar() => new(this);
}
