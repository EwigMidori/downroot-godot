namespace Downroot.Core.Save;

public sealed class SaveGameData
{
    public string SlotId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int WorldSeed { get; set; }
    public string ActiveWorldSpaceKind { get; set; } = string.Empty;
    public SavedPlayerData Player { get; set; } = new();
    public float TimeOfDaySeconds { get; set; }
    public float TotalElapsedSeconds { get; set; }
    public IReadOnlyList<SavedWorldRuntimeData> Worlds { get; set; } = [];
}
