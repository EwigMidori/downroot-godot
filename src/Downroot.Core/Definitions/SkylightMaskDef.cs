namespace Downroot.Core.Definitions;

public sealed record SkylightMaskDef(
    bool BlocksSkylight,
    LightingFootprintKind Footprint = LightingFootprintKind.None);
