using Downroot.Content.Registries;
using Downroot.Core.Gameplay;
using Downroot.World.Models;

namespace Downroot.Gameplay.Runtime;

public sealed class GameRuntime(ContentRegistrySet content, WorldModel world, WorldState worldState, PlayerState player, GameBootstrapConfig bootstrapConfig)
{
    public ContentRegistrySet Content { get; } = content;
    public WorldModel World { get; } = world;
    public WorldState WorldState { get; } = worldState;
    public PlayerState Player { get; } = player;
    public GameBootstrapConfig BootstrapConfig { get; } = bootstrapConfig;
}
