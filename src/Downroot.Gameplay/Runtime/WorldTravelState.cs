using Downroot.Core.World;

namespace Downroot.Gameplay.Runtime;

public sealed class WorldTravelState
{
    public bool IsActive => Phase != WorldTravelPhase.None;
    public bool InputLocked => IsActive;
    public WorldTravelPhase Phase { get; set; }
    public float PhaseRemainingSeconds { get; set; }
    public WorldSpaceKind SourceWorldSpaceKind { get; set; } = WorldSpaceKind.Overworld;
    public WorldSpaceKind TargetWorldSpaceKind { get; set; } = WorldSpaceKind.Overworld;
    public ChunkCoord SourcePortalChunk { get; set; }
    public WorldTileCoord SourcePortalTile { get; set; }
    public WorldTileCoord TargetPortalTile { get; set; }

    public float OverlayAlpha01
    {
        get
        {
            return Phase switch
            {
                WorldTravelPhase.FadingOut => 1f - (PhaseRemainingSeconds / 0.25f),
                WorldTravelPhase.Switching => 1f,
                WorldTravelPhase.FadingIn => PhaseRemainingSeconds / 0.25f,
                _ => 0f
            };
        }
    }

    public void Reset()
    {
        Phase = WorldTravelPhase.None;
        PhaseRemainingSeconds = 0f;
        SourceWorldSpaceKind = WorldSpaceKind.Overworld;
        TargetWorldSpaceKind = WorldSpaceKind.Overworld;
        SourcePortalChunk = default;
        SourcePortalTile = default;
        TargetPortalTile = default;
    }
}

public enum WorldTravelPhase
{
    None,
    FadingOut,
    Switching,
    FadingIn
}
