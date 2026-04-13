using Downroot.Core.Input;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Downroot.Game.Infrastructure;

public sealed class GodotInputService(Func<NumericsVector2> pointerProvider, Func<bool> isPointerOverUi) : IInputService
{
    public InputFrame CaptureFrame()
    {
        var movement = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        var pointer = pointerProvider();
        var pointerBlockedByUi = isPointerOverUi();

        return new InputFrame(
            new NumericsVector2(movement.X, movement.Y),
            new NumericsVector2(pointer.X, pointer.Y),
            !pointerBlockedByUi && Input.IsActionJustPressed("interact"),
            !pointerBlockedByUi && Input.IsMouseButtonPressed(MouseButton.Left),
            !pointerBlockedByUi && Input.IsMouseButtonPressed(MouseButton.Right),
            false,
            !pointerBlockedByUi && Input.IsActionJustPressed("toggle_craft_workspace"),
            !pointerBlockedByUi && Input.IsActionJustPressed("consume_selected"),
            GetScrollDelta(),
            GetDirectHotbarSlot());
    }

    private static int GetScrollDelta()
    {
        if (Input.IsActionJustPressed("hotbar_next"))
        {
            return 1;
        }

        if (Input.IsActionJustPressed("hotbar_prev"))
        {
            return -1;
        }

        return 0;
    }

    private static int? GetDirectHotbarSlot()
    {
        for (var index = 0; index < 8; index++)
        {
            if (Input.IsActionJustPressed($"hotbar_{index + 1}"))
            {
                return index;
            }
        }

        return null;
    }
}
