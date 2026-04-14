namespace Downroot.Gameplay.Runtime;

public sealed class PlaceableRuntimeState
{
    public bool IsOpen { get; set; }
    public bool IsLit { get; set; }
    public float FuelSecondsRemaining { get; set; }
    public bool AssignedAsPrimaryBed { get; set; }
    public float FuelLastUpdatedTotalSeconds { get; set; }

    public PlaceableRuntimeState Clone()
    {
        return new PlaceableRuntimeState
        {
            IsOpen = IsOpen,
            IsLit = IsLit,
            FuelSecondsRemaining = FuelSecondsRemaining,
            AssignedAsPrimaryBed = AssignedAsPrimaryBed,
            FuelLastUpdatedTotalSeconds = FuelLastUpdatedTotalSeconds
        };
    }
}
