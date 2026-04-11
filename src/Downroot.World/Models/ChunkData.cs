using Downroot.Core.Definitions;

namespace Downroot.World.Models;

public sealed class ChunkData
{
    private readonly TerrainDef?[,] _terrain;

    public ChunkData(int width, int height)
    {
        Width = width;
        Height = height;
        _terrain = new TerrainDef?[width, height];
    }

    public int Width { get; }
    public int Height { get; }

    public TerrainDef? GetTerrain(int x, int y) => _terrain[x, y];

    public void SetTerrain(int x, int y, TerrainDef terrainDef) => _terrain[x, y] = terrainDef;
}
