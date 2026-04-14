using Downroot.Core.Ids;
using Downroot.Core.World;

namespace Downroot.Gameplay.Runtime;

public sealed record DestroyTarget(
    bool IsRaisedFeature,
    WorldTileCoord Tile,
    ContentId ContentId,
    WorldSpaceKind WorldSpaceKind,
    ChunkCoord ChunkCoord,
    WorldEntityState? Entity = null);
