using Downroot.Core.World;

namespace Downroot.Gameplay.Runtime;

public sealed class DebugRuntimeState
{
    private GameRuntime? _runtime;

    public bool ShowChunkBounds { get; set; }
    public bool GodMode { get; set; }
    public bool FastBreak { get; set; }
    public string CurrentSaveName { get; private set; } = string.Empty;
    public string CurrentSlotId { get; private set; } = string.Empty;
    public int WorldSeed => _runtime?.WorldSeed ?? 0;

    public void Bind(GameRuntime runtime, string currentSaveName)
    {
        _runtime = runtime;
        CurrentSaveName = currentSaveName;
        CurrentSlotId = runtime.SaveSlotId ?? string.Empty;
    }

    public WorldSpaceKind CurrentWorldSpace => _runtime?.ActiveWorldSpaceKind ?? WorldSpaceKind.Overworld;

    public WorldTileCoord CurrentPlayerTile => _runtime?.GetWorldTile(_runtime.Player.Position) ?? default;

    public ChunkCoord CurrentChunk => _runtime?.GetChunkCoord(_runtime.Player.Position) ?? default;

    public int LoadedChunkCount => _runtime?.GetWorld(CurrentWorldSpace).LoadedChunks.Count ?? 0;

    public int ActiveEntityCount
    {
        get
        {
            if (_runtime is null)
            {
                return 0;
            }

            _runtime.WorldState.EnsureEntityProjectionCurrent();
            return _runtime.WorldState.Entities.Count;
        }
    }
}
