using Downroot.Core.Ids;

namespace Downroot.Core.World;

public sealed record RaisedOreFieldRuleDef(
    ContentId PassTargetFeatureId,
    WorldSpaceKind WorldSpaceKind,
    string RequiredSurfaceRegion,
    float DepositThreshold,
    bool AllowMixedOverworldOreSelection,
    IReadOnlyList<ContentId> MixedFeatureIds);
