using System.Numerics;
using Downroot.Core.Definitions;
using Downroot.Core.World;

namespace Downroot.Gameplay.Runtime.Systems;

public sealed class LightingInputCollector(GameRuntime runtime, WorldRuntimeFacade worldFacade)
{
    public LightingInputSnapshot Collect()
    {
        var world = worldFacade.GetActiveWorld();
        var emitters = new List<RuntimeLightEmitter>();
        var occluders = new List<RuntimeLightOccluder>();
        var skylightMasks = new List<RuntimeSkylightMask>();

        foreach (var entity in world.EnumerateLoadedEntities())
        {
            if (entity.Removed || entity.Kind != WorldEntityKind.Placeable)
            {
                continue;
            }

            if (!runtime.Content.Placeables.TryGet(entity.DefinitionId, out var placeableDef))
            {
                continue;
            }

            var tile = worldFacade.GetWorldTile(entity.Position);
            CollectEmitter(entity, placeableDef!, tile, emitters);
            CollectOccluder(entity, placeableDef!, tile, occluders);
            CollectSkylightMask(entity, placeableDef!, tile, skylightMasks);
        }

        return new LightingInputSnapshot(
            ResolveBounds(world),
            emitters,
            occluders,
            skylightMasks);
    }

    private void CollectEmitter(
        WorldEntityState entity,
        PlaceableDef placeableDef,
        WorldTileCoord tile,
        List<RuntimeLightEmitter> emitters)
    {
        if (placeableDef.LightEmitter is not { } lightEmitter)
        {
            return;
        }

        var isEnabled = ResolveEmitterEnabled(entity, placeableDef, lightEmitter);
        emitters.Add(new RuntimeLightEmitter(
            entity.Id,
            entity.WorldSpaceKind,
            tile,
            lightEmitter.RadiusTiles,
            lightEmitter.Intensity,
            new Vector3(lightEmitter.ColorR, lightEmitter.ColorG, lightEmitter.ColorB),
            lightEmitter.FlickerKind,
            isEnabled,
            lightEmitter.PresentationKind));
    }

    private void CollectOccluder(
        WorldEntityState entity,
        PlaceableDef placeableDef,
        WorldTileCoord tile,
        List<RuntimeLightOccluder> occluders)
    {
        if (placeableDef.LightOccluder is not { BlocksLight: true, Footprint: LightingFootprintKind.Tile })
        {
            return;
        }

        occluders.Add(new RuntimeLightOccluder(entity.Id, tile, true));
    }

    private void CollectSkylightMask(
        WorldEntityState entity,
        PlaceableDef placeableDef,
        WorldTileCoord tile,
        List<RuntimeSkylightMask> skylightMasks)
    {
        if (placeableDef.SkylightMask is not { BlocksSkylight: true, Footprint: LightingFootprintKind.Tile })
        {
            return;
        }

        skylightMasks.Add(new RuntimeSkylightMask(entity.Id, tile, true));
    }

    private static bool ResolveEmitterEnabled(WorldEntityState entity, PlaceableDef placeableDef, LightEmitterDef lightEmitter)
    {
        if (!placeableDef.HasBehavior(PlaceableBehaviorKind.LightSource))
        {
            return lightEmitter.EnabledByDefault;
        }

        return entity.PlaceableState?.IsLit ?? lightEmitter.EnabledByDefault;
    }

    private static LightingFieldBounds ResolveBounds(LoadedWorldState world)
    {
        if (world.LoadedChunks.Count == 0)
        {
            return new LightingFieldBounds(0, 0, 1, 1);
        }

        var minChunkX = world.LoadedChunks.Keys.Min(coord => coord.X);
        var maxChunkX = world.LoadedChunks.Keys.Max(coord => coord.X);
        var minChunkY = world.LoadedChunks.Keys.Min(coord => coord.Y);
        var maxChunkY = world.LoadedChunks.Keys.Max(coord => coord.Y);
        var chunk = world.LoadedChunks.Values.First();
        var chunkWidth = chunk.GeneratedChunk.Surface.Width;
        var chunkHeight = chunk.GeneratedChunk.Surface.Height;
        return new LightingFieldBounds(
            minChunkX * chunkWidth,
            minChunkY * chunkHeight,
            ((maxChunkX - minChunkX) + 1) * chunkWidth,
            ((maxChunkY - minChunkY) + 1) * chunkHeight);
    }
}

public readonly record struct LightingFieldBounds(int MinTileX, int MinTileY, int Width, int Height);

public sealed record LightingInputSnapshot(
    LightingFieldBounds Bounds,
    IReadOnlyList<RuntimeLightEmitter> Emitters,
    IReadOnlyList<RuntimeLightOccluder> Occluders,
    IReadOnlyList<RuntimeSkylightMask> SkylightMasks);
