using Downroot.Core.Ids;
using Downroot.Core.World;

namespace Downroot.Gameplay.Runtime;

public readonly record struct RuntimeLightOccluder(
    EntityId EntityId,
    WorldTileCoord WorldTile,
    bool BlocksLight);
