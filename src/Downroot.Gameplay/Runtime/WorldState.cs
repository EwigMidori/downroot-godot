using Downroot.Core.Ids;

namespace Downroot.Gameplay.Runtime;

public sealed class WorldState
{
    private readonly List<WorldEntityState> _entities = [];

    public IReadOnlyList<WorldEntityState> Entities => _entities;
    public float TimeOfDaySeconds { get; set; }
    public float TotalElapsedSeconds { get; set; }
    public bool InventoryVisible { get; set; }
    public bool CraftingVisible { get; set; }
    public string? ActiveStationKey { get; set; }
    public EntityId? ActiveStationEntityId { get; set; }
    public string InteractionPrompt { get; set; } = string.Empty;
    public float DestroyProgress01 { get; set; }

    public bool IsNight(float dayLengthSeconds) => TimeOfDaySeconds >= dayLengthSeconds * 0.5f;

    public void AddEntity(WorldEntityState entity) => _entities.Add(entity);

    public void RemoveDeleted() => _entities.RemoveAll(entity => entity.Removed);
}
