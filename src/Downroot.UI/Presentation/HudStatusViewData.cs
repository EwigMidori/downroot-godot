namespace Downroot.UI.Presentation;

public sealed record HudStatusViewData(
    string TimeOfDayLabel,
    bool IsNight,
    float HealthPercent,
    float HungerPercent);
