using Downroot.Content.Packs;
using Downroot.Content.Registries;
using Downroot.Gameplay.Runtime;
using Downroot.World.Generation;
using Downroot.World.Generation.Passes;

namespace Downroot.Gameplay.Bootstrap;

public sealed class GameBootstrapper
{
    public GameRuntime Bootstrap()
    {
        var registries = new ContentRegistrySet();
        var registrar = registries.CreateRegistrar();
        var locatedPacks = new ContentPackLocator().LocatePacks();

        foreach (var pack in locatedPacks)
        {
            pack.Register(registrar);
        }

        var worldGenerator = new WorldGenerator(
            registries,
            [
                new FillTerrainPass("basegame:grass"),
                new DirtPatchPass("basegame:dirt")
            ]);

        var world = worldGenerator.Generate(24, 16);
        var player = new PlayerState
        {
            Position = new System.Numerics.Vector2(24 * 16f, 16 * 16f)
        };

        return new GameRuntime(registries, world, player);
    }
}
