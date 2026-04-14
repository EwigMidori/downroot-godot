namespace Downroot.UI.Presentation;

public sealed class DebugPanelViewData
{
    public string CurrentWorldSpace { get; set; } = string.Empty;
    public string PlayerTile { get; set; } = string.Empty;
    public string CurrentChunk { get; set; } = string.Empty;
    public int LoadedChunkCount { get; set; }
    public int CurrentEntityCount { get; set; }
    public string CurrentSaveName { get; set; } = string.Empty;
    public int WorldSeed { get; set; }
    public bool ShowChunkBounds { get; set; }
    public bool GodMode { get; set; }
    public bool FastBreak { get; set; }
}
