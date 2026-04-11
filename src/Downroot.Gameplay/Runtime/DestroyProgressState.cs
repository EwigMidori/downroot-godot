using System.Numerics;
using Downroot.Core.Ids;

namespace Downroot.Gameplay.Runtime;

public sealed record DestroyProgressState(
    EntityId EntityId,
    WorldEntityKind EntityKind,
    ContentId ContentId,
    Vector2 WorldPosition,
    float Progress01);
