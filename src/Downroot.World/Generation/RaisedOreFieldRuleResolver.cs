using Downroot.Content.Registries;
using Downroot.Core.Ids;
using Downroot.Core.World;

namespace Downroot.World.Generation;

public sealed class RaisedOreFieldRuleResolver(ContentRegistrySet registries)
{
    public RaisedOreFieldRuleDef Resolve(ContentId passTargetFeatureId, WorldSpaceKind worldSpaceKind)
    {
        return registries.RaisedOreFieldRules.FirstOrDefault(rule =>
                   rule.PassTargetFeatureId == passTargetFeatureId && rule.WorldSpaceKind == worldSpaceKind)
               ?? throw new InvalidOperationException(
                   $"Missing raised ore field rule for feature '{passTargetFeatureId.Value}' in '{worldSpaceKind}'.");
    }

    public ContentId ResolveMixedFeature(RaisedOreFieldRuleDef rule, IWorldGenContext context, WorldTileCoord latticeCoord)
    {
        if (!rule.AllowMixedOverworldOreSelection || rule.MixedFeatureIds.Count == 0)
        {
            return rule.PassTargetFeatureId;
        }

        var roll = context.GetStableUnitValue(latticeCoord, 8161);
        var index = Math.Min(rule.MixedFeatureIds.Count - 1, (int)MathF.Floor(roll * rule.MixedFeatureIds.Count));
        return rule.MixedFeatureIds[index];
    }
}
