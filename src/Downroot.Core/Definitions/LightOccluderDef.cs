namespace Downroot.Core.Definitions;

public sealed record LightOccluderDef(
    bool BlocksLight,
    LightingFootprintKind Footprint = LightingFootprintKind.None);
