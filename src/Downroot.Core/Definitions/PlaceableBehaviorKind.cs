namespace Downroot.Core.Definitions;

[Flags]
public enum PlaceableBehaviorKind
{
    None = 0,
    Door = 1 << 0,
    Storage = 1 << 1,
    CraftingStation = 1 << 2,
    Bed = 1 << 3,
    LightSource = 1 << 4
}
