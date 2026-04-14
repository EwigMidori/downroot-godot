namespace Downroot.Core.World;

public readonly record struct WorldTileCoord(int X, int Y)
{
    public ChunkCoord ToChunkCoord(int chunkWidth, int chunkHeight)
    {
        return new ChunkCoord(FloorDiv(X, chunkWidth), FloorDiv(Y, chunkHeight));
    }

    public LocalTileCoord ToLocalCoord(int chunkWidth, int chunkHeight)
    {
        return new LocalTileCoord(Mod(X, chunkWidth), Mod(Y, chunkHeight));
    }

    public static WorldTileCoord FromChunkAndLocal(ChunkCoord chunkCoord, LocalTileCoord localCoord, int chunkWidth, int chunkHeight)
    {
        return new WorldTileCoord(
            (chunkCoord.X * chunkWidth) + localCoord.X,
            (chunkCoord.Y * chunkHeight) + localCoord.Y);
    }

    private static int FloorDiv(int value, int divisor)
    {
        if (divisor <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(divisor));
        }

        var quotient = value / divisor;
        var remainder = value % divisor;
        if (remainder != 0 && value < 0)
        {
            quotient--;
        }

        return quotient;
    }

    private static int Mod(int value, int divisor)
    {
        var remainder = value % divisor;
        return remainder < 0 ? remainder + divisor : remainder;
    }
}
