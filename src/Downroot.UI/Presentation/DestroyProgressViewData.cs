using System.Numerics;

namespace Downroot.UI.Presentation;

public sealed record DestroyProgressViewData(
    bool IsVisible,
    string DestroyTargetLabel,
    float Progress01,
    Vector2 WorldPosition);
