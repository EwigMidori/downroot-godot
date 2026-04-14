using System.Numerics;
using Downroot.Core.Definitions;

namespace Downroot.Gameplay.Runtime.Systems;

public sealed class CreatureSystem(GameRuntime runtime, WorldQueryService worldQuery, MovementSystem movementSystem, Action<int> damagePlayer)
{
    private enum CreatureIntent
    {
        Idle,
        Chase,
        Flee
    }

    public void UpdateCreatures(float deltaSeconds)
    {
        var isNight = runtime.WorldState.IsNight(runtime.BootstrapConfig.DayLengthSeconds);

        foreach (var creature in worldQuery.EnumerateActiveEntities().Where(entity => entity.Kind == WorldEntityKind.Creature && !entity.Removed))
        {
            var def = runtime.Content.Creatures.Get(creature.DefinitionId);
            var distance = Vector2.Distance(creature.Position, runtime.Player.Position);
            switch (ResolveIntent(def, isNight, distance, creature))
            {
                case CreatureIntent.Flee:
                    creature.Position = movementSystem.MoveWithCollision(
                        creature.Position,
                        MovementSystem.NormalizeMovement(creature.Position - runtime.Player.Position) * def.MoveSpeed * deltaSeconds,
                        creature.Id);
                    continue;
                case CreatureIntent.Idle:
                    continue;
                case CreatureIntent.Chase:
                    creature.Position = movementSystem.MoveWithCollision(
                        creature.Position,
                        MovementSystem.NormalizeMovement(runtime.Player.Position - creature.Position) * def.MoveSpeed * deltaSeconds,
                        creature.Id);
                    break;
            }

            creature.AiAccumulator += deltaSeconds;
            if (Vector2.Distance(creature.Position, runtime.Player.Position) < 18f && creature.AiAccumulator >= def.ContactDamageCooldownSeconds)
            {
                damagePlayer(def.ContactDamage);
                creature.AiAccumulator = 0f;
            }
        }
    }

    public WorldEntityState? GetNearestCreature(float range) => worldQuery.GetNearestCreature(range);

    public void DamageCreature(WorldEntityState creature, int amount)
    {
        creature.Durability = Math.Max(0, creature.Durability - amount);
        creature.AiAccumulator = 0f;
        creature.HitFlashSeconds = 0.16f;
        if (creature.Durability <= 0)
        {
            creature.Removed = true;
        }
    }

    private static bool CanAggroNow(CreatureDef def, bool isNight)
    {
        return isNight && def.NightAggroRange > 0f;
    }

    private static bool IsTargetInAggroRange(CreatureDef def, float distance)
    {
        return distance <= def.NightAggroRange;
    }

    private static CreatureIntent ResolveIntent(CreatureDef def, bool isNight, float distance, WorldEntityState creature)
    {
        if (!isNight && def.DayFleeStartRange > 0f)
        {
            if (distance <= def.DayFleeStartRange)
            {
                creature.OpenState = true;
            }
            else if (distance >= def.DayFleeStopRange)
            {
                creature.OpenState = false;
            }

            return creature.OpenState ? CreatureIntent.Flee : CreatureIntent.Idle;
        }

        if (CanAggroNow(def, isNight) && IsTargetInAggroRange(def, distance))
        {
            return CreatureIntent.Chase;
        }

        return CreatureIntent.Idle;
    }
}
