using Downroot.Core.Save;

namespace Downroot.Game.Infrastructure;

public sealed class SettingsRepository
{
    private readonly SavePathResolver _paths;
    private readonly JsonFileStore _store;

    public SettingsRepository(SavePathResolver paths, JsonFileStore store)
    {
        _paths = paths;
        _store = store;
    }

    public GameSettingsData Load()
    {
        return _store.Read<GameSettingsData>(_paths.GetSettingsPath()) ?? new GameSettingsData();
    }

    public void Save(GameSettingsData settings)
    {
        _store.Write(_paths.GetSettingsPath(), settings);
    }
}
