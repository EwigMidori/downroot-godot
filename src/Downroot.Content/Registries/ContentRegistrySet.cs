using Downroot.Core.Definitions;
using Downroot.Core.Registries;
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
    }

    public IRegistry<ItemDef> Items { get; }
    public IRegistry<PlaceableDef> Placeables { get; }
    public IRegistry<RecipeDef> Recipes { get; }
    public IRegistry<CreatureDef> Creatures { get; }
    public IRegistry<TerrainDef> Terrains { get; }

    public ContentRegistrar CreateRegistrar() => new(this);
}
