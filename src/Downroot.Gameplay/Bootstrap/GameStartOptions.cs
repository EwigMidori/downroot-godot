namespace Downroot.Gameplay.Bootstrap;

public sealed class GameStartOptions
{
    public string SaveSlotId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int WorldSeed { get; set; }
    public IReadOnlyList<string> EnabledPackIds { get; set; } = [];
    public bool IsNewGame { get; set; }
}
