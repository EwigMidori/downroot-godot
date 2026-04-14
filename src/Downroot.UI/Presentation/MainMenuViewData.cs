namespace Downroot.UI.Presentation;

public sealed class MainMenuViewData
{
    public bool CanContinue { get; set; }
    public bool CanLoadGame { get; set; }
    public string Heading { get; set; } = string.Empty;
    public string Subheading { get; set; } = string.Empty;
    public string VersionLabel { get; set; } = string.Empty;
}
