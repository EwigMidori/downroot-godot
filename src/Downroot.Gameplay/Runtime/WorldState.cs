using Downroot.Core.Ids;

namespace Downroot.Gameplay.Runtime;

public sealed class WorldState
{
    private readonly List<WorldEntityState> _entities = [];

    public IReadOnlyList<WorldEntityState> Entities => _entities;
    public float TimeOfDaySeconds { get; set; }
    public float TotalElapsedSeconds { get; set; }
    public CraftWorkspaceMode WorkspaceMode { get; set; }
    public string? ActiveStationKey { get; set; }
    public EntityId? ActiveStationEntityId { get; set; }
    public InteractionContext? CurrentInteraction { get; set; }
    public StatusEventState? ActiveStatusEvent { get; private set; }
    public float ActiveStatusEventSeconds { get; private set; }
    public DestroyProgressState? ActiveDestroyProgress { get; set; }
    public FurnaceTaskState? ActiveFurnaceTask { get; set; }
    public float PlayerHitFlashSeconds { get; set; }

    public bool IsNight(float dayLengthSeconds) => TimeOfDaySeconds >= dayLengthSeconds * 0.5f;

    public void AddEntity(WorldEntityState entity) => _entities.Add(entity);

    public void SetStatusEvent(StatusEventState statusEvent, float seconds = 2f)
    {
        ActiveStatusEvent = statusEvent;
        ActiveStatusEventSeconds = seconds;
    }

    public void TickStatusEvent(float deltaSeconds)
    {
        PlayerHitFlashSeconds = Math.Max(0f, PlayerHitFlashSeconds - deltaSeconds);

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

    public void RemoveDeleted() => _entities.RemoveAll(entity => entity.Removed);
}
