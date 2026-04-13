using System.Numerics;
using Downroot.Content.Packs;
using Downroot.Content.Registries;
using Downroot.Core.World;
using Downroot.Gameplay.Runtime;
using Downroot.World.Generation;

namespace Downroot.Gameplay.Bootstrap;

public sealed class GameBootstrapper
{
    public GameRuntime Bootstrap()
    {
        var registries = new ContentRegistrySet();
        var registrar = registries.CreateRegistrar();

        foreach (var pack in new ContentPackLocator().LocatePacks())
        {
            pack.Register(registrar);
        }

        var bootstrapConfig = registries.BootstrapConfig
            ?? throw new InvalidOperationException("No bootstrap config was registered by any content pack.");

        var world = new WorldGenerator(
            registries,
            registries.WorldGenPasses.Select(WorldGenPassFactory.Create).ToArray())
            .Generate(bootstrapConfig.WorldWidth, bootstrapConfig.WorldHeight);
        LogWorldGeneration(world);

        var player = new PlayerState(
            inventorySize: 16,
            hotbarSize: 8,
            survival: new SurvivalState(
                bootstrapConfig.StartingHealth,
                bootstrapConfig.MaxHealth,
                bootstrapConfig.StartingHunger,
                bootstrapConfig.MaxHunger))
        {
            Position = new Vector2(bootstrapConfig.PlayerSpawn.Tile.X * 32, bootstrapConfig.PlayerSpawn.Tile.Y * 32)
        };

        var worldState = new WorldState();
        foreach (var spawn in world.Spawns)
        {
            var position = new Vector2(spawn.Tile.X * 32, spawn.Tile.Y * 32);

            if (registries.ResourceNodes.TryGet(spawn.ContentId, out var resourceDef))
            {
                worldState.AddEntity(new WorldEntityState(WorldEntityKind.ResourceNode, resourceDef!.Id, position, resourceDef.MaxDurability));
                continue;
            }

            if (registries.Creatures.TryGet(spawn.ContentId, out var creatureDef))
            {
                worldState.AddEntity(new WorldEntityState(WorldEntityKind.Creature, creatureDef!.Id, position, creatureDef.MaxHealth));
            }
        }

        return new GameRuntime(registries, world, worldState, player, bootstrapConfig);
    }

    private static void LogWorldGeneration(Downroot.World.Models.WorldModel world)
    {
        var regionSummary = string.Join(
            ", ",
            world.Surface.CountSurfaceRegions()
                .OrderBy(pair => pair.Key)
                .Select(pair => $"{pair.Key}:{pair.Value}"));
        var spawnSummary = string.Join(
            ", ",
            world.Spawns.GroupBy(spawn => spawn.ContentId.Value)
                .OrderBy(group => group.Key)
                .Select(group => $"{group.Key}:{group.Count()}"));

        Console.WriteLine($"[WorldGen] regions => {regionSummary}");
        Console.WriteLine($"[WorldGen] spawns => {spawnSummary}");
    }
}
