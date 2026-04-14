namespace Downroot.Core.Save;

public sealed class SaveSlotSummary
{
    public string SlotId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int WorldSeed { get; set; }
    public DateTimeOffset LastWriteUtc { get; set; }
    public string CurrentWorldSpace { get; set; } = string.Empty;
    public int PlayerHealth { get; set; }
    public int PlayerHunger { get; set; }
}
