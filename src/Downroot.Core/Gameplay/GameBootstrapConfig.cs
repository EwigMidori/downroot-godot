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
    int DayLengthSeconds,
    int StartingHealth,
    int StartingHunger,
    int MaxHealth,
    int MaxHunger,
    TileSpawn PlayerSpawn,
    TileSpawn DebugPlaceableSpawn);
