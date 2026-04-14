namespace Downroot.Core.Save;

public sealed class SavedWorldRuntimeData
{
    public string WorldSpaceKind { get; set; } = string.Empty;
    public string StableWorldId { get; set; } = string.Empty;
    public int WorldSeed { get; set; }
    public IReadOnlyList<SavedChunkRuntimeData> Chunks { get; set; } = [];
}
