using Downroot.Core.Ids;

namespace Downroot.World.Models;

public sealed class ChunkData
{
    private readonly ContentId?[,] _terrain;

    public ChunkData(int width, int height)
    {
        Width = width;
        Height = height;
        _terrain = new ContentId?[width, height];
    }

    public int Width { get; }
    public int Height { get; }

    public ContentId? GetTerrainId(int x, int y) => _terrain[x, y];

    public void SetTerrain(int x, int y, ContentId terrainId) => _terrain[x, y] = terrainId;
}
