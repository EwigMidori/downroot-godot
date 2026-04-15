using System.Numerics;
using Downroot.Core.Definitions;
using Downroot.Core.Ids;
using Downroot.Core.World;

namespace Downroot.Gameplay.Runtime;

public readonly record struct RuntimeLightEmitter(
    EntityId EntityId,
    WorldSpaceKind WorldSpaceKind,
    WorldTileCoord WorldTile,
    float RadiusTiles,
    float Intensity,
    Vector3 Color,
    LightFlickerKind FlickerKind,
    bool IsEnabled,
    LightPresentationKind PresentationKind);
