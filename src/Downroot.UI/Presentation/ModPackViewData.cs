namespace Downroot.UI.Presentation;

public sealed class ModPackViewData
{
    public string PackId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IReadOnlyList<string> Dependencies { get; set; } = [];
    public bool IsBuiltIn { get; set; }
    public bool IsEnabled { get; set; }
    public bool CanToggle { get; set; }
    public string Status { get; set; } = string.Empty;
}
