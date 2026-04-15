using Downroot.Core.Gameplay;
using Downroot.Core.Ids;

namespace Downroot.Core.Definitions;

public sealed record PlaceableDef(
    ContentId Id,
    string DisplayName,
    string SourcePackId,
    string SpritePath,
    int SpriteWidth,
    int SpriteHeight,
    int AtlasColumn = 0,
    int AtlasRow = 0,
    int MaxDurability = 3,
    bool IsCraftingStation = false,
    CraftingStationKind? CraftingStationKind = null,
    bool BlocksMovement = false,
    bool HasOpenVariant = false,
    int OpenAtlasColumn = 0,
    int OpenAtlasRow = 0,
    bool BlocksMovementWhenOpen = false,
    bool IsGroundCover = false,
    bool CanBeDestroyed = true,
    int StorageSlotCount = 0,
    bool ConnectsToSameNeighbors = false,
    PlaceableBehaviorKind Behaviors = PlaceableBehaviorKind.None,
    LightEmitterDef? LightEmitter = null,
    LightOccluderDef? LightOccluder = null,
    SkylightMaskDef? SkylightMask = null) : ContentDef(Id, DisplayName, SourcePackId)
{
    public bool HasBehavior(PlaceableBehaviorKind behavior) => (Behaviors & behavior) == behavior;
}
