using Downroot.Content.Packs;
using Downroot.Content.Registries;
using Downroot.Gameplay.Runtime;
using Downroot.World.Generation;

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

        var bootstrapConfig = registries.BootstrapConfig
            ?? throw new InvalidOperationException("No bootstrap config was registered by any content pack.");

        var worldPasses = registries.WorldGenPasses
            .Select(WorldGenPassFactory.Create)
            .ToArray();

        var worldGenerator = new WorldGenerator(registries, worldPasses);

        var world = worldGenerator.Generate(bootstrapConfig.WorldWidth, bootstrapConfig.WorldHeight);
        var player = new PlayerState
        {
            Position = new System.Numerics.Vector2(
                bootstrapConfig.PlayerSpawn.Tile.X * 32 + bootstrapConfig.PlayerSpawn.PixelOffsetX,
                bootstrapConfig.PlayerSpawn.Tile.Y * 32 + bootstrapConfig.PlayerSpawn.PixelOffsetY)
        };

        return new GameRuntime(registries, world, player, bootstrapConfig);
    }
}
