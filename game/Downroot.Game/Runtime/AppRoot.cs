using Downroot.Core.Save;
using Downroot.Game.Infrastructure;
using Downroot.Gameplay.Bootstrap;
using Downroot.UI.Presentation;
using Godot;

namespace Downroot.Game.Runtime;

public partial class AppRoot : Control
{
    private SavePathResolver? _paths;
    private JsonFileStore? _store;
    private SaveGameRepository? _saves;
    private SettingsRepository? _settings;
    private SettingsApplier? _settingsApplier;
    private SessionController? _session;
    private MainMenuController? _mainMenu;
    private NewGameController? _newGame;
    private LoadGameController? _loadGame;
    private SettingsController? _settingsPage;
    private Control? _pageHost;
    private bool _pauseMenuActive;
    private Control? _currentPage;

    public override void _Ready()
    {
        ProcessMode = Node.ProcessModeEnum.Always;
        _paths = new SavePathResolver();
        _store = new JsonFileStore(_paths);
        _saves = new SaveGameRepository(_paths, _store);
        _settings = new SettingsRepository(_paths, _store);
        _settingsApplier = new SettingsApplier();
        _settingsApplier.Apply(_settings.Load());
        GameInputMapInstaller.Install();

        _pageHost = new Control { AnchorRight = 1, AnchorBottom = 1, Name = "AppPageHost", ProcessMode = Node.ProcessModeEnum.Always };
        AddChild(_pageHost);
        _session = new SessionController(this, _saves);

        _mainMenu = new MainMenuController();
        _mainMenu.ContinueRequested += HandleContinueRequested;
        _mainMenu.NewGameRequested += HandleNewGameRequested;
        _mainMenu.QuickStartRequested += HandleQuickStartRequested;
        _mainMenu.LoadGameRequested += HandleLoadGameRequested;
        _mainMenu.SettingsRequested += HandleSettingsRequested;
        _mainMenu.QuitRequested += HandleQuitRequested;

        _newGame = new NewGameController();
        _newGame.CreateRequested += CreateNewGame;
        _newGame.BackRequested += () => ShowMainMenu();

        _loadGame = new LoadGameController();
        _loadGame.LoadRequested += LoadSlot;
        _loadGame.DeleteRequested += DeleteSlot;
        _loadGame.BackRequested += () => ShowMainMenu();

        _settingsPage = new SettingsController();
        _settingsPage.ApplyRequested += ApplySettings;
        _settingsPage.BackRequested += HandleSettingsBackRequested;

        ShowMainMenu();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true, Echo: false, Keycode: Key.Escape })
        {
            return;
        }

        if (_session?.GameRoot is null)
        {
            return;
        }

        if (!_pageHost!.Visible)
        {
            ShowPauseMenu();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (_pauseMenuActive && ReferenceEquals(_currentPage, _mainMenu?.View))
        {
            ResumeSession();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (_pauseMenuActive)
        {
            ShowPauseMenu();
            GetViewport().SetInputAsHandled();
        }
    }

    private void ShowMainMenu()
    {
        GetTree().Paused = false;
        _pauseMenuActive = false;
        _session!.Stop(saveBeforeClose: true);
        _pageHost!.Visible = true;
        _mainMenu!.Bind(new MainMenuViewData
        {
            CanContinue = _saves!.LoadManifest().LastPlayedSlotId is not null,
            CanLoadGame = _saves.ListSlots().Count > 0,
            VersionLabel = $"v{ProjectSettings.GetSetting("application/config/version", "0.4")}"
        });
        ShowPage(_mainMenu.View);
    }

    private void ShowPauseMenu()
    {
        if (_session?.GameRoot is null)
        {
            return;
        }

        GetTree().Paused = true;
        _pauseMenuActive = true;
        _pageHost!.Visible = true;
        _mainMenu!.BindPauseMenu(_session.CurrentSlotId is not null);
        ShowPage(_mainMenu.View);
    }

    private void ResumeSession()
    {
        _pauseMenuActive = false;
        _pageHost!.Visible = false;
        GetTree().Paused = false;
    }

    private void ShowLoadGame()
    {
        _loadGame!.Bind(_saves!.ListSlots().Select(slot => new SaveSlotViewData
        {
            SlotId = slot.SlotId,
            DisplayName = slot.DisplayName,
            WorldSeed = slot.WorldSeed,
            CurrentWorldSpace = slot.CurrentWorldSpace,
            PlayerHealth = slot.PlayerHealth,
            PlayerHunger = slot.PlayerHunger,
            LastWriteUtc = slot.LastWriteUtc
        }).ToArray());
        ShowPage(_loadGame.View);
    }

    private void ShowSettings()
    {
        var current = _settings!.Load();
        _settingsPage!.Bind(new SettingsViewData
        {
            Fullscreen = current.Fullscreen,
            VSync = current.VSync,
            MasterVolume = current.MasterVolume,
            UiScale = current.UiScale
        });
        ShowPage(_settingsPage.View);
    }

    private void ContinueLastSave()
    {
        var last = _saves!.LoadManifest().LastPlayedSlotId;
        if (string.IsNullOrWhiteSpace(last))
        {
            return;
        }

        LoadSlot(last);
    }

    private void QuickStart()
    {
        var displayName = $"Quick Start {DateTime.Now:yyyy-MM-dd HHmmss}";
        var slotId = _saves!.CreateSlotId(displayName);
        StartSession(new GameBootstrapRequest
        {
            StartOptions = new GameStartOptions
            {
                SaveSlotId = slotId,
                DisplayName = displayName,
                WorldSeed = Random.Shared.Next(),
                IsNewGame = true
            }
        });
    }

    private void CreateNewGame(string displayName, string seedText)
    {
        var resolvedName = string.IsNullOrWhiteSpace(displayName) ? "New World" : displayName.Trim();
        var slotId = _saves!.CreateSlotId(resolvedName);
        StartSession(new GameBootstrapRequest
        {
            StartOptions = new GameStartOptions
            {
                SaveSlotId = slotId,
                DisplayName = resolvedName,
                WorldSeed = ResolveSeed(seedText),
                IsNewGame = true
            }
        });
    }

    private void LoadSlot(string slotId)
    {
        var save = _saves!.LoadSave(slotId);
        if (save is null)
        {
            ShowLoadGame();
            return;
        }

        StartSession(new GameBootstrapRequest
        {
            StartOptions = new GameStartOptions
            {
                SaveSlotId = save.SlotId,
                DisplayName = save.DisplayName,
                WorldSeed = save.WorldSeed,
                IsNewGame = false
            },
            ExistingSave = save
        });
    }

    private void DeleteSlot(string slotId)
    {
        _saves!.DeleteSave(slotId);
        ShowLoadGame();
    }

    private void ApplySettings(GameSettingsData settings)
    {
        _settings!.Save(settings);
        _settingsApplier!.Apply(settings);
    }

    private void StartSession(GameBootstrapRequest request)
    {
        GetTree().Paused = false;
        _pauseMenuActive = false;
        _pageHost!.Visible = false;
        _session!.Start(request);
    }

    private void ShowPage(Control page)
    {
        foreach (var child in _pageHost!.GetChildren())
        {
            _pageHost.RemoveChild(child);
        }

        if (page.GetParent() is Node parent)
        {
            parent.RemoveChild(page);
        }

        _pageHost.AddChild(page);
        MoveChild(_pageHost, GetChildCount() - 1);
        _currentPage = page;
    }

    private void HandleContinueRequested()
    {
        if (_pauseMenuActive)
        {
            ResumeSession();
            return;
        }

        ContinueLastSave();
    }

    private void HandleNewGameRequested()
    {
        if (_pauseMenuActive)
        {
            _session?.SaveCurrent();
            return;
        }

        ShowPage(_newGame!.View);
    }

    private void HandleQuickStartRequested()
    {
        if (_pauseMenuActive)
        {
            return;
        }

        QuickStart();
    }

    private void HandleLoadGameRequested()
    {
        if (_pauseMenuActive)
        {
            ShowMainMenu();
            return;
        }

        ShowLoadGame();
    }

    private void HandleSettingsRequested()
    {
        ShowSettings();
    }

    private void HandleQuitRequested()
    {
        if (_pauseMenuActive)
        {
            _session?.Stop(saveBeforeClose: true);
        }

        GetTree().Quit();
    }

    private void HandleSettingsBackRequested()
    {
        if (_pauseMenuActive)
        {
            ShowPauseMenu();
            return;
        }

        ShowMainMenu();
    }

    private static int ResolveSeed(string seedText)
    {
        if (string.IsNullOrWhiteSpace(seedText))
        {
            return Random.Shared.Next();
        }

        if (int.TryParse(seedText.Trim(), out var numeric))
        {
            return numeric;
        }

        unchecked
        {
            var hash = 17;
            foreach (var character in seedText.Trim())
            {
                hash = (hash * 31) + character;
            }

            return hash;
        }
    }
}
