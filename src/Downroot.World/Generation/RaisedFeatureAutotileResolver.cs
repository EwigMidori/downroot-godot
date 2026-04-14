using Downroot.World.Models;

namespace Downroot.World.Generation;

public static class RaisedFeatureAutotileResolver
{
    public static RaisedFeatureVariant Resolve(
        bool north,
        bool east,
        bool south,
        bool west,
        bool northEast,
        bool northWest,
        bool southEast,
        bool southWest)
    {
        if (north && east && south && west)
        {
            if (!northWest)
            {
                return RaisedFeatureVariant.InnerCornerNorthWest;
            }

            if (!northEast)
            {
                return RaisedFeatureVariant.InnerCornerNorthEast;
            }

            if (!southWest)
            {
                return RaisedFeatureVariant.InnerCornerSouthWest;
            }

            if (!southEast)
            {
                return RaisedFeatureVariant.InnerCornerSouthEast;
            }

            return RaisedFeatureVariant.Solid;
        }

        if (north && east && south)
        {
            return RaisedFeatureVariant.TeeMissingWest;
        }

        if (north && west && south)
        {
            return RaisedFeatureVariant.TeeMissingEast;
        }

        if (east && south && west)
        {
            return RaisedFeatureVariant.TeeMissingNorth;
        }

        if (north && east && west)
        {
            return RaisedFeatureVariant.TeeMissingSouth;
        }

        if (south && east)
        {
            return RaisedFeatureVariant.CornerSouthEast;
        }

        if (south && west)
        {
            return RaisedFeatureVariant.CornerSouthWest;
        }

        if (north && east)
        {
            return RaisedFeatureVariant.CornerNorthEast;
        }

        if (north && west)
        {
            return RaisedFeatureVariant.CornerNorthWest;
        }

        throw new InvalidOperationException("Unsupported raised feature autotile mask reached resolver.");
    }
}
