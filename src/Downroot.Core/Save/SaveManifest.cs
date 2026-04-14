namespace Downroot.Core.Save;

public sealed class SaveManifest
{
    public IReadOnlyList<SaveSlotSummary> Slots { get; set; } = [];
    public string? LastPlayedSlotId { get; set; }
}
