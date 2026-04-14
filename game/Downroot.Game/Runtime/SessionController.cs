using Downroot.Core.Save;
using Downroot.Game.Infrastructure;
using Downroot.Gameplay.Bootstrap;
using Downroot.Gameplay.Persistence;
using Downroot.Gameplay.Runtime;
using Godot;

namespace Downroot.Game.Runtime;

public sealed class SessionController
{
    private readonly Node _host;
    private readonly SaveGameRepository _saveRepository;
    private readonly GameBootstrapper _bootstrapper = new();
    private readonly GameSaveSnapshotBuilder _snapshotBuilder = new();

    private GameRoot? _gameRoot;
    private GameBootstrapRequest? _currentRequest;
    private DebugRuntimeState? _debugState;

    public SessionController(Node host, SaveGameRepository saveRepository)
    {
        _host = host;
        _saveRepository = saveRepository;
    }

    public GameRoot? GameRoot => _gameRoot;
    public GameRuntime? Runtime => _gameRoot is null ? null : _gameRoot.Runtime;
    public DebugRuntimeState? DebugState => _debugState;
    public string? CurrentSlotId => Runtime?.SaveSlotId;
    public string? CurrentDisplayName => Runtime?.SaveDisplayName;

    public void Start(GameBootstrapRequest request)
    {
        Stop(saveBeforeClose: false);
        _currentRequest = request;
        var runtime = _bootstrapper.Bootstrap(request);
        _saveRepository.SetLastPlayedSlot(request.StartOptions.SaveSlotId);
        _debugState = new DebugRuntimeState();
        _debugState.Bind(runtime, request.StartOptions.DisplayName);
        _gameRoot = new GameRoot
        {
            ProcessMode = Node.ProcessModeEnum.Pausable
        };
        _gameRoot.Configure(runtime, _debugState, SaveCurrent, ReloadCurrent);
        _host.AddChild(_gameRoot);

        if (request.StartOptions.IsNewGame)
        {
            SaveCurrent();
        }
    }

    public void Stop(bool saveBeforeClose)
    {
        if (saveBeforeClose && Runtime?.SaveSlotId is not null)
        {
            SaveCurrent();
        }

        if (_gameRoot is not null)
        {
            _gameRoot.QueueFree();
            _gameRoot = null;
        }

        _debugState = null;
    }

    public void SaveCurrent()
    {
        if (Runtime?.SaveSlotId is null)
        {
            return;
        }

        var save = _snapshotBuilder.Build(Runtime);
        _saveRepository.SaveGame(save);
    }

    public void ReloadCurrent()
    {
        if (Runtime?.SaveSlotId is not { } slotId)
        {
            return;
        }

        var existingSave = _saveRepository.LoadSave(slotId);
        if (existingSave is null)
        {
            return;
        }

        Start(new GameBootstrapRequest
        {
            StartOptions = new GameStartOptions
            {
                SaveSlotId = existingSave.SlotId,
                DisplayName = existingSave.DisplayName,
                WorldSeed = existingSave.WorldSeed,
                IsNewGame = false
            },
            ExistingSave = existingSave
        });
    }
}
