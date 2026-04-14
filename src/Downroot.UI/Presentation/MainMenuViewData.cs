namespace Downroot.UI.Presentation;

public sealed class MainMenuViewData
{
    public bool CanContinue { get; set; }
    public bool CanLoadGame { get; set; }
    public string VersionLabel { get; set; } = string.Empty;
}
