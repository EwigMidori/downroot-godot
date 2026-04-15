using Downroot.Content.Packs;
using Downroot.Core.Save;

namespace Downroot.Game.Infrastructure;

public sealed class ModSettingsRepository
{
    private readonly SavePathResolver _paths;
    private readonly JsonFileStore _store;

    public ModSettingsRepository(SavePathResolver paths, JsonFileStore store)
    {
        _paths = paths;
        _store = store;
    }

    public GameModSettingsData Load()
    {
        return _store.Read<GameModSettingsData>(_paths.GetModSettingsPath())
            ?? new GameModSettingsData
            {
                EnabledPackIds = [BaseGameContentPack.Id, PortalModContentPack.Id]
            };
    }

    public void Save(GameModSettingsData settings)
    {
        _store.Write(_paths.GetModSettingsPath(), settings);
    }
}
