namespace Downroot.Gameplay.Runtime;

public sealed class EntityProjectionBuilder
{
    public IReadOnlyList<WorldEntityState> Build(LoadedWorldState world)
    {
        return world.EnumerateLoadedEntities()
            .Where(entity => !entity.Removed)
            .OrderBy(entity => entity.Position.Y)
            .ThenBy(entity => entity.Position.X)
            .ToArray();
    }
}
