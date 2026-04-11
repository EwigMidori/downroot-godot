using System.Numerics;
using Downroot.Core.Ids;

namespace Downroot.Gameplay.Runtime;

public sealed class PlayerState
{
    public PlayerState()
    {
        Id = EntityId.New();
    }

    public EntityId Id { get; }
    public Vector2 Position { get; set; }
    public float Speed { get; set; } = 180f;
}
