using Downroot.Content.Registries;
using Downroot.Core.Gameplay;
using Downroot.Gameplay.Bootstrap;
using Downroot.Core.World;
using Downroot.World.Generation;
using System.Numerics;

namespace Downroot.Gameplay.Runtime;

public sealed class GameRuntime(
    ContentRegistrySet content,
    WorldGenerator overworldGenerator,
    WorldGenerator dimShardGenerator,
    WorldState worldState,
    PlayerState player,
    GameBootstrapConfig bootstrapConfig)
{
    public ContentRegistrySet Content { get; } = content;
    public WorldGenerator OverworldGenerator { get; } = overworldGenerator;
    public WorldGenerator DimShardGenerator { get; } = dimShardGenerator;
    public WorldState WorldState { get; } = worldState;
    public PlayerState Player { get; } = player;
    public GameBootstrapConfig BootstrapConfig { get; } = bootstrapConfig;
    public GameStartOptions? StartOptions { get; set; }
    public string? SaveSlotId => StartOptions?.SaveSlotId;
    public string? SaveDisplayName => StartOptions?.DisplayName;
    public int WorldSeed => StartOptions?.WorldSeed ?? BootstrapConfig.WorldSeed;

    public WorldSpaceKind ActiveWorldSpaceKind
    {
        get => WorldState.ActiveWorldSpaceKind;
        set => WorldState.ActiveWorldSpaceKind = value;
    }

    public LoadedWorldState Overworld => WorldState.Overworld;
    public LoadedWorldState DimShardPocket => WorldState.DimShardPocket;
    public int ChunkWidth => BootstrapConfig.ChunkWidth;
    public int ChunkHeight => BootstrapConfig.ChunkHeight;

    public LoadedWorldState GetWorld(WorldSpaceKind worldSpaceKind)
    {
        return worldSpaceKind == WorldSpaceKind.Overworld
            ? Overworld
            : DimShardPocket;
    }

    public WorldGenerator GetWorldGenerator(WorldSpaceKind worldSpaceKind)
    {
        return worldSpaceKind == WorldSpaceKind.Overworld
            ? OverworldGenerator
            : DimShardGenerator;
    }

    public WorldTileCoord GetWorldTile(Vector2 worldPosition)
    {
        return new WorldTileCoord(
            (int)MathF.Floor(worldPosition.X / 32f),
            (int)MathF.Floor(worldPosition.Y / 32f));
    }

    public ChunkCoord GetChunkCoord(Vector2 worldPosition) => GetWorldTile(worldPosition).ToChunkCoord(ChunkWidth, ChunkHeight);

    public Vector2 GetWorldPosition(WorldTileCoord tileCoord) => new(tileCoord.X * 32f, tileCoord.Y * 32f);
}
