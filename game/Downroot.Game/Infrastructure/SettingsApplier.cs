using Downroot.Core.Save;
using Godot;

namespace Downroot.Game.Infrastructure;

public sealed class SettingsApplier
{
    public void Apply(GameSettingsData settings)
    {
        DisplayServer.WindowSetMode(settings.Fullscreen
            ? DisplayServer.WindowMode.Fullscreen
            : DisplayServer.WindowMode.Windowed);
        DisplayServer.WindowSetVsyncMode(settings.VSync
            ? DisplayServer.VSyncMode.Enabled
            : DisplayServer.VSyncMode.Disabled);

        var masterBus = AudioServer.GetBusIndex("Master");
        if (masterBus >= 0)
        {
            var volume = Mathf.Clamp(settings.MasterVolume, 0f, 1f);
            AudioServer.SetBusVolumeDb(masterBus, Mathf.LinearToDb(Mathf.Max(volume, 0.0001f)));
        }

        if (Engine.GetMainLoop() is SceneTree tree)
        {
            tree.Root.ContentScaleFactor = Mathf.Clamp(settings.UiScale, 0.75f, 1.75f);
        }
    }
}
