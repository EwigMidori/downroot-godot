using System.Numerics;
using Downroot.Core.Definitions;
using Downroot.Core.Input;
using Downroot.Core.World;

namespace Downroot.Gameplay.Runtime.Systems;

public sealed class DestroySystem(GameRuntime runtime, WorldRuntimeFacade worldFacade, WorldQueryService worldQuery)
{
    private const float InteractionRange = 48f;
    private readonly Dictionary<WorldTileCoord, float> _raisedFeatureDamage = [];

    public void HandleDestroy(float deltaSeconds, InputFrame input, bool suppressDestroyUntilRelease, bool suppressBecauseAttacking, ItemDef? selectedItemDef, bool fastBreak)
    {
        if (suppressDestroyUntilRelease || suppressBecauseAttacking)
        {
            runtime.WorldState.ActiveDestroyProgress = null;
            return;
        }

        var target = GetNearestDestroyTarget();
        if (target is null || !input.DestroyHeld)
        {
            runtime.WorldState.ActiveDestroyProgress = null;
            return;
        }

        var breakDuration = fastBreak ? 0.05f : GetBreakDuration(target, selectedItemDef);
        var progressValue = target.IsRaisedFeature
            ? AddRaisedFeatureDamage(target.Tile, deltaSeconds)
            : AddEntityDamage(target.Entity!, deltaSeconds);
        var progress = Math.Clamp(progressValue / breakDuration, 0f, 1f);
        runtime.WorldState.ActiveDestroyProgress = new DestroyProgressState(
            target.Entity?.Id,
            target.Entity?.Kind,
            target.IsRaisedFeature,
            target.Tile,
            target.ContentId,
            worldFacade.GetWorldPosition(target.Tile),
            progress);
        if (progressValue < breakDuration)
        {
            return;
        }

        DestroyResolvedTarget(target);
        runtime.WorldState.ActiveDestroyProgress = null;
    }

    public DestroyTarget? GetNearestDestroyTarget()
    {
        var raised = GetNearestRaisedFeatureTarget();
        if (raised is not null)
        {
            return raised;
        }

        var entity = worldQuery.GetNearestDestructibleEntity(InteractionRange);
        return entity is null
            ? null
            : new DestroyTarget(false, worldFacade.GetWorldTile(entity.Position), entity.DefinitionId, entity.WorldSpaceKind, entity.ChunkCoord, entity);
    }

    private DestroyTarget? GetNearestRaisedFeatureTarget()
    {
        var world = worldFacade.GetActiveWorld();
        var centerTile = worldFacade.GetWorldTile(runtime.Player.Position);
        var tileRadius = (int)MathF.Ceiling(InteractionRange / 32f);
        DestroyTarget? best = null;
        var bestDistance = float.MaxValue;

        for (var y = centerTile.Y - tileRadius; y <= centerTile.Y + tileRadius; y++)
        {
            for (var x = centerTile.X - tileRadius; x <= centerTile.X + tileRadius; x++)
            {
                var tile = new WorldTileCoord(x, y);
                var featureId = world.GetRaisedFeatureId(tile, runtime.ChunkWidth, runtime.ChunkHeight);
                if (featureId is null)
                {
                    continue;
                }

                var distance = Vector2.Distance(worldFacade.GetWorldPosition(tile), runtime.Player.Position);
                if (distance > InteractionRange || distance >= bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                best = new DestroyTarget(
                    true,
                    tile,
                    featureId.Value,
                    runtime.ActiveWorldSpaceKind,
                    tile.ToChunkCoord(runtime.ChunkWidth, runtime.ChunkHeight));
            }
        }

        return best;
    }

    private void DestroyResolvedTarget(DestroyTarget target)
    {
        if (target.IsRaisedFeature)
        {
            DestroyRaisedFeature(target);
            return;
        }

        DestroyEntity(target.Entity!);
    }

    private void DestroyRaisedFeature(DestroyTarget target)
    {
        if (!worldFacade.TryGetChunk(target.WorldSpaceKind, target.ChunkCoord, out _))
        {
            return;
        }

        worldFacade.RemoveRaisedFeature(target.WorldSpaceKind, target.Tile);
        _raisedFeatureDamage.Remove(target.Tile);
        var raisedFeature = runtime.Content.RaisedFeatures.Get(target.ContentId);
        foreach (var drop in raisedFeature.Drops)
        {
            worldFacade.AddRuntimeEntity(target.WorldSpaceKind, new WorldEntityState(
                WorldEntityKind.ItemDrop,
                drop.ItemId,
                worldFacade.GetWorldPosition(target.Tile),
                1,
                target.WorldSpaceKind,
                target.ChunkCoord,
                stackCount: drop.Amount));
        }
    }

    private void DestroyEntity(WorldEntityState entity)
    {
        if (runtime.WorldState.ActiveStationEntityId == entity.Id)
        {
            runtime.WorldState.ActiveStationEntityId = null;
            runtime.WorldState.ActiveStationKind = null;
            runtime.WorldState.WorkspaceMode = CraftWorkspaceMode.Hidden;
        }

        if (runtime.WorldState.ActiveStorageEntityId == entity.Id)
        {
            runtime.WorldState.ActiveStorageEntityId = null;
        }

        entity.Removed = true;
        worldFacade.NotifyEntityStateChanged(entity);

        if (entity.StorageInventory is not null)
        {
            var worldTile = worldFacade.GetWorldTile(entity.Position);
            foreach (var slot in entity.StorageInventory.Slots.Where(slot => slot.ItemId is not null && slot.Quantity > 0))
            {
                worldFacade.AddRuntimeEntity(entity.WorldSpaceKind, new WorldEntityState(
                    WorldEntityKind.ItemDrop,
                    slot.ItemId!.Value,
                    entity.Position,
                    1,
                    entity.WorldSpaceKind,
                    worldTile.ToChunkCoord(runtime.ChunkWidth, runtime.ChunkHeight),
                    stackCount: slot.Quantity));
            }
        }

        switch (entity.Kind)
        {
            case WorldEntityKind.ResourceNode:
                var resourceDef = runtime.Content.ResourceNodes.Get(entity.DefinitionId);
                foreach (var drop in resourceDef.Drops)
                {
                    var worldTile = worldFacade.GetWorldTile(entity.Position);
                    worldFacade.AddRuntimeEntity(entity.WorldSpaceKind, new WorldEntityState(
                        WorldEntityKind.ItemDrop,
                        drop.ItemId,
                        entity.Position,
                        1,
                        entity.WorldSpaceKind,
                        worldTile.ToChunkCoord(runtime.ChunkWidth, runtime.ChunkHeight),
                        stackCount: drop.Amount));
                }
                break;
            case WorldEntityKind.Placeable:
                var itemDef = runtime.Content.Items.All.FirstOrDefault(item => item.PlaceableId == entity.DefinitionId);
                if (itemDef is not null)
                {
                    var worldTile = worldFacade.GetWorldTile(entity.Position);
                    worldFacade.AddRuntimeEntity(entity.WorldSpaceKind, new WorldEntityState(
                        WorldEntityKind.ItemDrop,
                        itemDef.Id,
                        entity.Position,
                        1,
                        entity.WorldSpaceKind,
                        worldTile.ToChunkCoord(runtime.ChunkWidth, runtime.ChunkHeight)));
                }
                break;
        }
    }

    private static float AddEntityDamage(WorldEntityState entity, float deltaSeconds)
    {
        entity.DamageAccumulator += deltaSeconds;
        return entity.DamageAccumulator;
    }

    private float AddRaisedFeatureDamage(WorldTileCoord tile, float deltaSeconds)
    {
        var current = _raisedFeatureDamage.GetValueOrDefault(tile);
        current += deltaSeconds;
        _raisedFeatureDamage[tile] = current;
        return current;
    }

    private float GetBreakDuration(DestroyTarget target, ItemDef? selectedItemDef)
    {
        if (!target.IsRaisedFeature)
        {
            return GetBreakDuration(target.Entity!, selectedItemDef);
        }

        var raisedFeature = runtime.Content.RaisedFeatures.Get(target.ContentId);
        return Math.Max(1, raisedFeature.MaxDurability);
    }

    private float GetBreakDuration(WorldEntityState target, ItemDef? selectedItemDef)
    {
        var duration = Math.Max(1, target.Durability);
        if (target.Kind != WorldEntityKind.ResourceNode)
        {
            return duration;
        }

        var resourceDef = runtime.Content.ResourceNodes.Get(target.DefinitionId);
        if (!resourceDef.IsTree)
        {
            return duration;
        }

        var multiplier = selectedItemDef?.TreeBreakSpeedMultiplier ?? 1f;
        return Math.Max(0.2f, duration / Math.Max(1f, multiplier));
    }
}
