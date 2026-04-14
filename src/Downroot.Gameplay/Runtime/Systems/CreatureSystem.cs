using System.Numerics;

namespace Downroot.Gameplay.Runtime.Systems;

public sealed class CreatureSystem(GameRuntime runtime, WorldQueryService worldQuery, MovementSystem movementSystem, Action<int> damagePlayer)
{
    public void UpdateCreatures(float deltaSeconds)
    {
        var isNight = runtime.WorldState.IsNight(runtime.BootstrapConfig.DayLengthSeconds);

        foreach (var creature in worldQuery.EnumerateActiveEntities().Where(entity => entity.Kind == WorldEntityKind.Creature && !entity.Removed))
        {
            var def = runtime.Content.Creatures.Get(creature.DefinitionId);
            var distance = Vector2.Distance(creature.Position, runtime.Player.Position);

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

                if (creature.OpenState && distance > 0f)
                {
                    var fleeDirection = Vector2.Normalize(creature.Position - runtime.Player.Position);
                    creature.Position = movementSystem.MoveWithCollision(creature.Position, fleeDirection * def.MoveSpeed * deltaSeconds, creature.Id);
                }
                continue;
            }

            var chase = (def.NightOnlyAggro && isNight) || (def.NightAggroRange > 0f && isNight && distance <= def.NightAggroRange);
            if (!chase)
            {
                continue;
            }

            var direction = runtime.Player.Position - creature.Position;
            if (direction != Vector2.Zero)
            {
                creature.Position = movementSystem.MoveWithCollision(creature.Position, Vector2.Normalize(direction) * def.MoveSpeed * deltaSeconds, creature.Id);
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
}
