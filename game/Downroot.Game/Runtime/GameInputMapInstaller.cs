using Godot;

namespace Downroot.Game.Runtime;

public static class GameInputMapInstaller
{
    public static void Install()
    {
        EnsureAction("move_left", Key.A);
        EnsureAction("move_right", Key.D);
        EnsureAction("move_up", Key.W);
        EnsureAction("move_down", Key.S);
        EnsureAction("interact", Key.F);
        EnsureAction("toggle_craft_workspace", Key.E);
        EnsureAction("consume_selected", Key.Q);

        EnsureMouseAction("hotbar_next", MouseButton.WheelDown);
        EnsureMouseAction("hotbar_prev", MouseButton.WheelUp);

        EnsureAction("hotbar_1", Key.Key1);
        EnsureAction("hotbar_2", Key.Key2);
        EnsureAction("hotbar_3", Key.Key3);
        EnsureAction("hotbar_4", Key.Key4);
        EnsureAction("hotbar_5", Key.Key5);
        EnsureAction("hotbar_6", Key.Key6);
        EnsureAction("hotbar_7", Key.Key7);
        EnsureAction("hotbar_8", Key.Key8);
    }

    private static void EnsureAction(string actionName, Key key)
    {
        if (!InputMap.HasAction(actionName))
        {
            InputMap.AddAction(actionName);
        }

        if (InputMap.ActionGetEvents(actionName).OfType<InputEventKey>().Any(existing => existing.PhysicalKeycode == key))
        {
            return;
        }

        InputMap.ActionAddEvent(actionName, new InputEventKey { PhysicalKeycode = key });
    }

    private static void EnsureMouseAction(string actionName, MouseButton button)
    {
        if (!InputMap.HasAction(actionName))
        {
            InputMap.AddAction(actionName);
        }

        if (InputMap.ActionGetEvents(actionName).OfType<InputEventMouseButton>().Any(existing => existing.ButtonIndex == button))
        {
            return;
        }

        InputMap.ActionAddEvent(actionName, new InputEventMouseButton { ButtonIndex = button });
    }
}
