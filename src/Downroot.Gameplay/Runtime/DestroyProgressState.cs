using System.Numerics;
using Downroot.Core.Ids;
using Downroot.Core.World;

namespace Downroot.Gameplay.Runtime;

public sealed record DestroyProgressState(
    EntityId? EntityId,
    WorldEntityKind? EntityKind,
    bool IsRaisedFeature,
    WorldTileCoord Tile,
    ContentId ContentId,
    Vector2 WorldPosition,
    float Progress01);
