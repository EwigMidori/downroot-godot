using Downroot.Core.Ids;
using Downroot.Core.Gameplay;
using Downroot.Core.World;

namespace Downroot.Gameplay.Runtime;

public sealed class WorldState
{
    private readonly List<WorldEntityState> _entities = [];
    private readonly EntityProjectionBuilder _projectionBuilder = new();

    // Active-world convenience projection for renderer, UI, and query services.
    public IReadOnlyList<WorldEntityState> Entities => _entities;
    public WorldSpaceKind ActiveWorldSpaceKind { get; set; } = WorldSpaceKind.Overworld;
    public required LoadedWorldState Overworld { get; init; }
    public required LoadedWorldState DimShardPocket { get; init; }
    public WorldTravelState Travel { get; } = new();
    public float TimeOfDaySeconds { get; set; }
    public float TotalElapsedSeconds { get; set; }
    public CraftWorkspaceMode WorkspaceMode { get; set; }
    public CraftingStationKind? ActiveStationKind { get; set; }
    public EntityId? ActiveStationEntityId { get; set; }
    public InteractionContext? CurrentInteraction { get; set; }
    public StatusEventState? ActiveStatusEvent { get; private set; }
    public float ActiveStatusEventSeconds { get; private set; }
    public DestroyProgressState? ActiveDestroyProgress { get; set; }
    public FurnaceTaskState? ActiveFurnaceTask { get; set; }
    public float PlayerHitFlashSeconds { get; set; }

    public bool IsNight(float dayLengthSeconds) => TimeOfDaySeconds >= dayLengthSeconds * 0.5f;

    public LoadedWorldState GetActiveWorld()
    {
        return ActiveWorldSpaceKind == WorldSpaceKind.Overworld
            ? Overworld
            : DimShardPocket;
    }

    public void RefreshEntityProjection()
    {
        RebuildEntityProjection(_projectionBuilder);
    }

    public void RebuildEntityProjection(EntityProjectionBuilder builder)
    {
        _entities.Clear();
        _entities.AddRange(builder.Build(GetActiveWorld()));
    }

    public void SetStatusEvent(StatusEventState statusEvent, float seconds = 2f)
    {
        ActiveStatusEvent = statusEvent;
        ActiveStatusEventSeconds = seconds;
    }

    public void TickStatusEvent(float deltaSeconds)
    {
        PlayerHitFlashSeconds = Math.Max(0f, PlayerHitFlashSeconds - deltaSeconds);
        foreach (var entity in _entities)
        {
            entity.HitFlashSeconds = Math.Max(0f, entity.HitFlashSeconds - deltaSeconds);
        }

        if (ActiveStatusEventSeconds <= 0f)
        {
            return;
        }

        ActiveStatusEventSeconds = Math.Max(0f, ActiveStatusEventSeconds - deltaSeconds);
        if (ActiveStatusEventSeconds <= 0f)
        {
            ActiveStatusEvent = null;
        }
    }

    public void RemoveDeleted()
    {
        foreach (var world in new[] { Overworld, DimShardPocket })
        {
            foreach (var chunk in world.LoadedChunks.Values)
            {
                foreach (var removedNatural in chunk.NaturalEntities.Values.Where(entity => entity.Removed).ToArray())
                {
                    chunk.RemoveEntity(removedNatural);
                }

                foreach (var removedRuntime in chunk.RuntimeEntities.Values.Where(entity => entity.Removed).ToArray())
                {
                    chunk.RemoveEntity(removedRuntime);
                }
            }
        }

        RefreshEntityProjection();
    }
}
