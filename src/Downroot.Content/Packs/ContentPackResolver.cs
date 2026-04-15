using Downroot.Core.Content;

namespace Downroot.Content.Packs;

public sealed class ContentPackResolver
{
    private readonly ContentPackLocator _locator;

    public ContentPackResolver()
        : this(new ContentPackLocator())
    {
    }

    public ContentPackResolver(ContentPackLocator locator)
    {
        _locator = locator;
    }

    public ResolvedContentPackSet Resolve(IEnumerable<string>? enabledPackIds)
    {
        var availablePacks = _locator.LocatePacks()
            .ToDictionary(pack => pack.PackId, StringComparer.Ordinal);
        var requested = new HashSet<string>(StringComparer.Ordinal) { BaseGameContentPack.Id };

        if (enabledPackIds is not null)
        {
            foreach (var packId in enabledPackIds.Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id.Trim()))
            {
                if (!availablePacks.ContainsKey(packId))
                {
                    throw new InvalidOperationException($"Unknown content pack '{packId}'.");
                }

                requested.Add(packId);
            }
        }

        var visited = new Dictionary<string, VisitState>(StringComparer.Ordinal);
        var ordered = new List<IContentPack>();
        foreach (var packId in requested.OrderBy(static id => id, StringComparer.Ordinal))
        {
            Visit(packId);
        }

        return new ResolvedContentPackSet
        {
            OrderedPacks = ordered,
            ById = ordered.ToDictionary(pack => pack.PackId, StringComparer.Ordinal)
        };

        void Visit(string packId)
        {
            if (!availablePacks.TryGetValue(packId, out var pack))
            {
                throw new InvalidOperationException($"Missing required content pack '{packId}'.");
            }

            if (visited.TryGetValue(packId, out var state))
            {
                if (state == VisitState.Visiting)
                {
                    throw new InvalidOperationException($"Cyclic content pack dependency detected at '{packId}'.");
                }

                return;
            }

            visited[packId] = VisitState.Visiting;
            foreach (var dependencyId in pack.Manifest.Dependencies)
            {
                if (string.IsNullOrWhiteSpace(dependencyId))
                {
                    continue;
                }

                if (!availablePacks.ContainsKey(dependencyId))
                {
                    throw new InvalidOperationException($"Content pack '{packId}' requires missing dependency '{dependencyId}'.");
                }

                requested.Add(dependencyId);
                Visit(dependencyId);
            }

            visited[packId] = VisitState.Visited;
            ordered.Add(pack);
        }
    }

    private enum VisitState
    {
        Visiting,
        Visited
    }
}
