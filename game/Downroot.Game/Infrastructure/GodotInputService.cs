using Downroot.Core.Input;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Downroot.Game.Infrastructure;

public sealed class GodotInputService : IInputService
{
    public NumericsVector2 GetMovementVector()
    {
        var direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        return new NumericsVector2(direction.X, direction.Y);
    }
}
