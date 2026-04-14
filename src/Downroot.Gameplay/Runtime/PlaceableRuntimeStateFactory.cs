using Downroot.Core.Definitions;

namespace Downroot.Gameplay.Runtime;

public static class PlaceableRuntimeStateFactory
{
    public static PlaceableRuntimeState? Create(GameRuntime runtime, PlaceableDef placeableDef)
    {
        if (placeableDef.Behaviors == PlaceableBehaviorKind.None)
        {
            return null;
        }

        var state = new PlaceableRuntimeState();
        if (placeableDef.HasBehavior(PlaceableBehaviorKind.LightSource))
        {
            state.IsLit = true;
            state.FuelSecondsRemaining = GetDefaultFuelSeconds(runtime);
            state.FuelLastUpdatedTotalSeconds = runtime.WorldState.TotalElapsedSeconds;
        }

        return state;
    }

    public static float GetDefaultFuelSeconds(GameRuntime runtime) => runtime.BootstrapConfig.DayLengthSeconds * 1.5f;
}
