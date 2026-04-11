using Downroot.Core.Ids;

namespace Downroot.Core.World;

public interface IWorldGenContext
{
    int Width { get; }
    int Height { get; }
    bool HasTerrain(ContentId contentId);
    void SetTerrain(TileCoord coord, ContentId terrainId);
}
