namespace Downroot.Gameplay.Runtime;

public sealed class EntityProjectionBuilder
{
    public IReadOnlyList<WorldEntityState> Build(LoadedWorldState world)
    {
        var projection = new List<WorldEntityState>();
        foreach (var entity in world.EnumerateLoadedEntities())
        {
            if (!entity.Removed)
            {
                projection.Add(entity);
            }
        }

        projection.Sort(static (left, right) =>
        {
            var y = left.Position.Y.CompareTo(right.Position.Y);
            return y != 0 ? y : left.Position.X.CompareTo(right.Position.X);
        });
        return projection;
    }
}
