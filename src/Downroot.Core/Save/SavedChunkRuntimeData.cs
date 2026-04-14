namespace Downroot.Core.Save;

public sealed class SavedChunkRuntimeData
{
    public int ChunkX { get; set; }
    public int ChunkY { get; set; }
    public IReadOnlyList<string> DestroyedNaturalEntityIds { get; set; } = [];
    public IReadOnlyList<string> CollectedNaturalDropIds { get; set; } = [];
    public IReadOnlyList<string> RemovedRaisedFeatureTiles { get; set; } = [];
    public IReadOnlyList<SavedRuntimeEntityData> RuntimeEntities { get; set; } = [];
}
