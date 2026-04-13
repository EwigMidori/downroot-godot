using Downroot.Core.Ids;

namespace Downroot.World.Models;

public sealed class ChunkData
{
    private readonly SurfaceCell[,] _cells;

    public ChunkData(int width, int height)
    {
        Width = width;
        Height = height;
        _cells = new SurfaceCell[width, height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                _cells[x, y] = new SurfaceCell();
            }
        }
    }

    public int Width { get; }
    public int Height { get; }

    public ContentId? GetTerrainId(int x, int y) => _cells[x, y].ResolvedTerrainId;

    public ContentId? GetBaseTerrainId(int x, int y) => _cells[x, y].BaseTerrainId;

    public string GetSurfaceRegion(int x, int y) => _cells[x, y].SurfaceRegion;

    public bool HasSurfaceRegion(int x, int y, string regionKey) => _cells[x, y].SurfaceRegion == regionKey;

    public void SetBaseTerrain(int x, int y, ContentId terrainId) => _cells[x, y].BaseTerrainId = terrainId;

    public void SetCoverTerrain(int x, int y, ContentId? terrainId) => _cells[x, y].CoverTerrainId = terrainId;

    public void SetSurfaceRegion(int x, int y, string regionKey) => _cells[x, y].SurfaceRegion = regionKey;

    public IDictionary<string, int> CountSurfaceRegions()
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var region = _cells[x, y].SurfaceRegion;
                if (!counts.TryAdd(region, 1))
                {
                    counts[region]++;
                }
            }
        }

        return counts;
    }
}
