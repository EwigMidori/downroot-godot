namespace Downroot.UI.Presentation;

public sealed class ModManagementViewData
{
    public IReadOnlyList<ModPackViewData> Packs { get; set; } = [];
    public string? ErrorMessage { get; set; }
}
