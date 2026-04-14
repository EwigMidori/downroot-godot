namespace Downroot.Core.World;

public sealed record PortalWorldLinkDef(
    WorldSpaceKind SourceWorldSpaceKind,
    WorldSpaceKind TargetWorldSpaceKind,
    ChunkCoord SourcePortalChunk,
    ChunkCoord TargetPortalChunk,
    string StableLinkId);
