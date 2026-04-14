using Downroot.UI.Presentation;
using Downroot.Gameplay.Runtime;
using Godot;

namespace Downroot.Game.Runtime;

public sealed class DebugPanelController
{
    private readonly CanvasLayer _layer;
    private readonly PanelContainer _panel;
    private readonly RichTextLabel _status;
    private readonly CheckBox _chunkBounds;
    private readonly CheckBox _godMode;
    private readonly CheckBox _fastBreak;
    private readonly LineEdit _commandInput;
    private readonly RichTextLabel _log;
    private readonly DebugRuntimeState _debugState;
    private readonly DebugCommandExecutor _executor;

    public DebugPanelController(Node host, DebugRuntimeState debugState, DebugCommandExecutor executor)
    {
        _debugState = debugState;
        _executor = executor;

        _layer = new CanvasLayer { Visible = false };
        _panel = new PanelContainer
        {
            Position = new Vector2(12, 12),
            Size = new Vector2(360, 380)
        };
        _layer.AddChild(_panel);
        host.AddChild(_layer);

        var stack = new VBoxContainer();
        stack.AddThemeConstantOverride("separation", 8);
        _panel.AddChild(stack);
        _status = new RichTextLabel { CustomMinimumSize = new Vector2(320, 120), FitContent = true };
        stack.AddChild(_status);

        _chunkBounds = new CheckBox { Text = "Show Chunk Bounds", FocusMode = Control.FocusModeEnum.None };
        _godMode = new CheckBox { Text = "God Mode", FocusMode = Control.FocusModeEnum.None };
        _fastBreak = new CheckBox { Text = "Fast Break", FocusMode = Control.FocusModeEnum.None };
        _chunkBounds.Toggled += value => _debugState.ShowChunkBounds = value;
        _godMode.Toggled += value => _debugState.GodMode = value;
        _fastBreak.Toggled += value => _debugState.FastBreak = value;
        stack.AddChild(_chunkBounds);
        stack.AddChild(_godMode);
        stack.AddChild(_fastBreak);

        _commandInput = new LineEdit { PlaceholderText = "Command", FocusMode = Control.FocusModeEnum.All };
        _commandInput.TextSubmitted += SubmitCommand;
        stack.AddChild(_commandInput);

        _log = new RichTextLabel { CustomMinimumSize = new Vector2(320, 140), FitContent = true };
        stack.AddChild(_log);
    }

    public bool Visible => _layer.Visible;

    public void ToggleVisibility()
    {
        _layer.Visible = !_layer.Visible;
        if (_layer.Visible)
        {
            _commandInput.GrabFocus();
        }
        else
        {
            _commandInput.ReleaseFocus();
        }
    }

    public void Refresh()
    {
        if (!_layer.Visible)
        {
            return;
        }

        var data = new DebugPanelViewData
        {
            CurrentWorldSpace = _debugState.CurrentWorldSpace.ToString(),
            PlayerTile = $"{_debugState.CurrentPlayerTile.X},{_debugState.CurrentPlayerTile.Y}",
            CurrentChunk = $"{_debugState.CurrentChunk.X},{_debugState.CurrentChunk.Y}",
            LoadedChunkCount = _debugState.LoadedChunkCount,
            CurrentEntityCount = _debugState.ActiveEntityCount,
            CurrentSaveName = _debugState.CurrentSaveName,
            WorldSeed = _debugState.WorldSeed,
            ShowChunkBounds = _debugState.ShowChunkBounds,
            GodMode = _debugState.GodMode,
            FastBreak = _debugState.FastBreak
        };

        _status.Text =
            $"World: {data.CurrentWorldSpace}\n" +
            $"Tile: {data.PlayerTile}\n" +
            $"Chunk: {data.CurrentChunk}\n" +
            $"Loaded Chunks: {data.LoadedChunkCount}\n" +
            $"Entities: {data.CurrentEntityCount}\n" +
            $"Save: {data.CurrentSaveName}\n" +
            $"Seed: {data.WorldSeed}";
        _chunkBounds.ButtonPressed = data.ShowChunkBounds;
        _godMode.ButtonPressed = data.GodMode;
        _fastBreak.ButtonPressed = data.FastBreak;
    }

    private void SubmitCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return;
        }

        var result = _executor.Execute(command);
        _log.Text = $"> {command}\n{result}\n\n{_log.Text}";
        _commandInput.Clear();
        _commandInput.GrabFocus();
    }
}
