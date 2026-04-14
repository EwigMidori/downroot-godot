using Downroot.Core.Save;

namespace Downroot.Gameplay.Bootstrap;

public sealed class GameBootstrapRequest
{
    public required GameStartOptions StartOptions { get; init; }
    public SaveGameData? ExistingSave { get; init; }
}
