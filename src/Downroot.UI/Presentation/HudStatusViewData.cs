namespace Downroot.UI.Presentation;

public sealed record HudStatusViewData(
    string TimeOfDayLabel,
    bool IsNight,
    float NightOverlayAlpha,
    float HealthPercent,
    float HungerPercent,
    float PlayerHitFlashAlpha);
