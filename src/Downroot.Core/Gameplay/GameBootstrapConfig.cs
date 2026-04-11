using Downroot.Core.Ids;

namespace Downroot.Core.Gameplay;

public sealed record GameBootstrapConfig(
    int WorldWidth,
    int WorldHeight,
    ContentId DefaultTerrainId,
    ContentId PlayerCreatureId,
    ContentId DebugItemId,
    ContentId DebugPlaceableId,
    ContentId DebugTerrainVariantId,
    TileSpawn PlayerSpawn,
    TileSpawn DebugPlaceableSpawn);
