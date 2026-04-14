using Downroot.Core.Save;
using Downroot.Gameplay.Runtime;

namespace Downroot.Gameplay.Persistence;

public sealed class WorldRuntimePersistenceAdapter
{
    public SavedWorldRuntimeData Export(LoadedWorldState world)
    {
        return new SavedWorldRuntimeData
        {
            WorldSpaceKind = world.WorldSpaceKind.ToString(),
            StableWorldId = world.Model.StableId,
            WorldSeed = world.WorldSeed,
            Chunks = world.ExportPersistedChunks()
        };
    }

    public void Import(LoadedWorldState world, SavedWorldRuntimeData savedWorld)
    {
        world.ImportPersistedChunks(savedWorld.Chunks);
    }
}
