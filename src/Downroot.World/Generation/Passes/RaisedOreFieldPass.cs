using Downroot.Core.Ids;
using Downroot.Core.World;
using Downroot.World.Models;

namespace Downroot.World.Generation.Passes;

public sealed class RaisedOreFieldPass(ContentId featureId) : IWorldGenPass
{
    private static readonly int[] AreaBuckets = [9, 16, 25, 36, 49, 64, 81, 100, 144, 196, 256, 400, 576, 784, 1024];

    public string Name => "raised-ore-field";

    public void Execute(IWorldGenContext context)
    {
        var occupancy = new Dictionary<WorldTileCoord, bool>();
        foreach (var worldTile in EnumerateHalo(context))
        {
            occupancy[worldTile] = SampleRawOccupancy(context, worldTile);
        }

        LegalizeOccupancy(occupancy);

        for (var y = 0; y < context.Height; y++)
        {
            for (var x = 0; x < context.Width; x++)
            {
                var local = new LocalTileCoord(x, y);
                var world = context.GetWorldTileCoord(local);
                if (!occupancy.TryGetValue(world, out var occupied) || !occupied)
                {
                    context.ClearRaisedFeature(local);
                    continue;
                }

                context.SetRaisedFeature(local, featureId);
                var variant = RaisedFeatureAutotileResolver.Resolve(
                    IsOccupied(occupancy, new WorldTileCoord(world.X, world.Y - 1)),
                    IsOccupied(occupancy, new WorldTileCoord(world.X + 1, world.Y)),
                    IsOccupied(occupancy, new WorldTileCoord(world.X, world.Y + 1)),
                    IsOccupied(occupancy, new WorldTileCoord(world.X - 1, world.Y)),
                    IsOccupied(occupancy, new WorldTileCoord(world.X + 1, world.Y - 1)),
                    IsOccupied(occupancy, new WorldTileCoord(world.X - 1, world.Y - 1)),
                    IsOccupied(occupancy, new WorldTileCoord(world.X + 1, world.Y + 1)),
                    IsOccupied(occupancy, new WorldTileCoord(world.X - 1, world.Y + 1)));
                context.SetRaisedFeatureVariantIndex(local, (byte)variant);
            }
        }
    }

    private IEnumerable<WorldTileCoord> EnumerateHalo(IWorldGenContext context)
    {
        for (var y = -1; y <= context.Height; y++)
        {
            for (var x = -1; x <= context.Width; x++)
            {
                yield return context.GetWorldTileCoord(new LocalTileCoord(x, y));
            }
        }
    }

    private bool SampleRawOccupancy(IWorldGenContext context, WorldTileCoord worldTile)
    {
        if (!MatchesWorldAndRegion(context, worldTile))
        {
            return false;
        }

        var cellX = FloorDiv(worldTile.X, 18);
        var cellY = FloorDiv(worldTile.Y, 18);
        for (var gy = cellY - 2; gy <= cellY + 2; gy++)
        {
            for (var gx = cellX - 2; gx <= cellX + 2; gx++)
            {
                var latticeCoord = new WorldTileCoord(gx, gy);
                if (!TrySampleDeposit(context, latticeCoord, out var deposit) || deposit.FeatureId != featureId)
                {
                    continue;
                }

                var dx = worldTile.X - deposit.Center.X;
                var dy = worldTile.Y - deposit.Center.Y;
                var normalizedDistance = ((dx * dx) / (deposit.RadiusX * deposit.RadiusX)) + ((dy * dy) / (deposit.RadiusY * deposit.RadiusY));
                if (normalizedDistance > 1f)
                {
                    continue;
                }

                var edgeNoise = SampleNoise(context, worldTile, deposit.ShapeSeed);
                var threshold = 0.82f + (edgeNoise * 0.28f);
                if (normalizedDistance <= threshold)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool MatchesWorldAndRegion(IWorldGenContext context, WorldTileCoord worldTile)
    {
        if (context.WorldSpaceKind == WorldSpaceKind.Overworld)
        {
            if (featureId.Value == "basegame:frostcore_raised")
            {
                return false;
            }

            return SampleSurfaceRegion(context, worldTile) == SurfaceRegions.DirtField;
        }

        if (context.WorldSpaceKind != WorldSpaceKind.DimShardPocket || featureId.Value != "basegame:frostcore_raised")
        {
            return false;
        }

        return SampleSurfaceRegion(context, worldTile) == SurfaceRegions.DimShardField;
    }

    private string SampleSurfaceRegion(IWorldGenContext context, WorldTileCoord worldTile)
    {
        if (context.WorldSpaceKind == WorldSpaceKind.DimShardPocket)
        {
            return SurfaceRegions.DimShardField;
        }

        var grass = GrassRegionPass.SampleLayeredNoise(context, worldTile);
        if (grass >= 0.57f)
        {
            return SurfaceRegions.GrassField;
        }

        var isRiver = RiverPass.IsRiverTile(context, worldTile);
        return isRiver ? SurfaceRegions.River : SurfaceRegions.DirtField;
    }

    private bool TrySampleDeposit(IWorldGenContext context, WorldTileCoord latticeCoord, out Deposit deposit)
    {
        var roll = context.GetStableUnitValue(latticeCoord, 8101);
        var threshold = context.WorldSpaceKind == WorldSpaceKind.Overworld ? 0.48f : 0.24f;
        if (roll > threshold)
        {
            deposit = default;
            return false;
        }

        var depositFeatureId = ResolveDepositFeatureId(context, latticeCoord);
        var area = ResolveAreaBucket(context.GetStableUnitValue(latticeCoord, 8107));
        var center = new WorldTileCoord(
            (latticeCoord.X * 18) + 9 + (int)MathF.Round((context.GetStableUnitValue(latticeCoord, 8113) - 0.5f) * 8f),
            (latticeCoord.Y * 18) + 9 + (int)MathF.Round((context.GetStableUnitValue(latticeCoord, 8123) - 0.5f) * 8f));
        var radius = MathF.Sqrt(area / MathF.PI);
        deposit = new Deposit(
            depositFeatureId,
            center,
            MathF.Max(2.5f, radius * (0.8f + (context.GetStableUnitValue(latticeCoord, 8131) * 0.55f))),
            MathF.Max(2.5f, radius * (0.8f + (context.GetStableUnitValue(latticeCoord, 8141) * 0.55f))),
            context.GetStableHash(latticeCoord, 8153));
        return true;
    }

    private ContentId ResolveDepositFeatureId(IWorldGenContext context, WorldTileCoord latticeCoord)
    {
        if (context.WorldSpaceKind == WorldSpaceKind.DimShardPocket)
        {
            return featureId;
        }

        var roll = context.GetStableUnitValue(latticeCoord, 8161);
        return roll switch
        {
            < 0.4f => new ContentId("basegame:voidite_raised"),
            < 0.7f => new ContentId("basegame:goldvein_raised"),
            _ => new ContentId("basegame:venomite_raised")
        };
    }

    private static int ResolveAreaBucket(float roll)
    {
        if (roll < 0.12f) return AreaBuckets[0];
        if (roll < 0.24f) return AreaBuckets[1];
        if (roll < 0.36f) return AreaBuckets[2];
        if (roll < 0.48f) return AreaBuckets[3];
        if (roll < 0.58f) return AreaBuckets[4];
        if (roll < 0.67f) return AreaBuckets[5];
        if (roll < 0.75f) return AreaBuckets[6];
        if (roll < 0.82f) return AreaBuckets[7];
        if (roll < 0.88f) return AreaBuckets[8];
        if (roll < 0.92f) return AreaBuckets[9];
        if (roll < 0.95f) return AreaBuckets[10];
        if (roll < 0.97f) return AreaBuckets[11];
        if (roll < 0.985f) return AreaBuckets[12];
        if (roll < 0.995f) return AreaBuckets[13];
        return AreaBuckets[14];
    }

    private static float SampleNoise(IWorldGenContext context, WorldTileCoord worldTile, int salt)
    {
        return context.GetStableUnitValue(worldTile, salt)
            * 0.6f
            + context.GetStableUnitValue(new WorldTileCoord(worldTile.X / 2, worldTile.Y / 2), salt + 17) * 0.4f;
    }

    private static void LegalizeOccupancy(IDictionary<WorldTileCoord, bool> occupancy)
    {
        for (var iteration = 0; iteration < 3; iteration++)
        {
            var remove = new List<WorldTileCoord>();
            var add = new List<WorldTileCoord>();
            foreach (var pair in occupancy.Where(pair => pair.Value).ToArray())
            {
                var coord = pair.Key;
                var north = IsOccupied(occupancy, new WorldTileCoord(coord.X, coord.Y - 1));
                var east = IsOccupied(occupancy, new WorldTileCoord(coord.X + 1, coord.Y));
                var south = IsOccupied(occupancy, new WorldTileCoord(coord.X, coord.Y + 1));
                var west = IsOccupied(occupancy, new WorldTileCoord(coord.X - 1, coord.Y));
                var cardinalCount = Count(north, east, south, west);
                if (cardinalCount == 0)
                {
                    remove.Add(coord);
                    continue;
                }

                var diagonalOnly = cardinalCount == 0 && Count(
                    IsOccupied(occupancy, new WorldTileCoord(coord.X - 1, coord.Y - 1)),
                    IsOccupied(occupancy, new WorldTileCoord(coord.X + 1, coord.Y - 1)),
                    IsOccupied(occupancy, new WorldTileCoord(coord.X + 1, coord.Y + 1)),
                    IsOccupied(occupancy, new WorldTileCoord(coord.X - 1, coord.Y + 1))) > 0;
                if (diagonalOnly)
                {
                    remove.Add(coord);
                }
            }

            foreach (var coord in occupancy.Keys.ToArray())
            {
                if (occupancy[coord])
                {
                    continue;
                }

                if (IsOccupied(occupancy, new WorldTileCoord(coord.X, coord.Y - 1))
                    && IsOccupied(occupancy, new WorldTileCoord(coord.X + 1, coord.Y))
                    && IsOccupied(occupancy, new WorldTileCoord(coord.X, coord.Y + 1))
                    && IsOccupied(occupancy, new WorldTileCoord(coord.X - 1, coord.Y)))
                {
                    add.Add(coord);
                }
            }

            foreach (var coord in remove)
            {
                occupancy[coord] = false;
            }

            foreach (var coord in add)
            {
                occupancy[coord] = true;
            }
        }
    }

    private static bool IsOccupied(IDictionary<WorldTileCoord, bool> occupancy, WorldTileCoord coord)
    {
        return occupancy.TryGetValue(coord, out var occupied) && occupied;
    }

    private static int Count(params bool[] values) => values.Count(value => value);

    private static int FloorDiv(int value, int divisor)
    {
        var quotient = value / divisor;
        var remainder = value % divisor;
        return remainder != 0 && value < 0 ? quotient - 1 : quotient;
    }

    private readonly record struct Deposit(ContentId FeatureId, WorldTileCoord Center, float RadiusX, float RadiusY, int ShapeSeed);
}
