using Downroot.Core.Ids;

namespace Downroot.World.Models;

public sealed class SurfaceCell
{
    public ContentId? BaseTerrainId { get; set; }
    public ContentId? CoverTerrainId { get; set; }
    public string SurfaceRegion { get; set; } = string.Empty;

    public ContentId? ResolvedTerrainId => CoverTerrainId ?? BaseTerrainId;
}
