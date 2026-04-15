namespace Downroot.Core.Definitions;

public sealed record LightEmitterDef(
    bool EnabledByDefault,
    float RadiusTiles,
    float Intensity,
    float ColorR,
    float ColorG,
    float ColorB,
    LightFlickerKind FlickerKind = LightFlickerKind.None,
    LightPresentationKind PresentationKind = LightPresentationKind.None);
