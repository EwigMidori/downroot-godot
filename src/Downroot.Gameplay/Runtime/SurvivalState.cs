namespace Downroot.Gameplay.Runtime;

public sealed class SurvivalState(int health, int maxHealth, int hunger, int maxHunger)
{
    public int Health { get; private set; } = health;
    public int MaxHealth { get; } = maxHealth;
    public int Hunger { get; private set; } = hunger;
    public int MaxHunger { get; } = maxHunger;

    public void RestoreHunger(int amount) => Hunger = Math.Min(MaxHunger, Hunger + amount);

    public void DrainHunger(int amount) => Hunger = Math.Max(0, Hunger - amount);

    public void Damage(int amount) => Health = Math.Max(0, Health - amount);

    public void Heal(int amount) => Health = Math.Min(MaxHealth, Health + amount);

    public void SetHealth(int value) => Health = Math.Clamp(value, 0, MaxHealth);

    public void SetHunger(int value) => Hunger = Math.Clamp(value, 0, MaxHunger);
}
