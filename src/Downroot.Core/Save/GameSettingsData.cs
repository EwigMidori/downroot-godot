namespace Downroot.Core.Save;

public sealed class GameSettingsData
{
    public bool Fullscreen { get; set; }
    public bool VSync { get; set; } = true;
    public float MasterVolume { get; set; } = 1f;
    public float UiScale { get; set; } = 1f;
}
