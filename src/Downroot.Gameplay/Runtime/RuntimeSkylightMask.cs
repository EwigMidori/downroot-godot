using Downroot.Core.Ids;
using Downroot.Core.World;

namespace Downroot.Gameplay.Runtime;

public readonly record struct RuntimeSkylightMask(
    EntityId EntityId,
    WorldTileCoord WorldTile,
    bool BlocksSkylight);
