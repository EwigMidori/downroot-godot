using Downroot.Core.Definitions;
using Downroot.Core.Ids;
using Downroot.Core.Registries;

namespace Downroot.Content.Registries;

public sealed class Registry<T> : IRegistry<T> where T : ContentDef
{
    private readonly Dictionary<ContentId, T> _entries = new();

    public IEnumerable<T> All => _entries.Values;

    public int Count => _entries.Count;

    public T Get(ContentId id)
    {
        if (!TryGet(id, out var entry))
        {
            throw new KeyNotFoundException($"Content entry '{id}' was not registered.");
        }

        return entry!;
    }

    public void Register(T entry)
    {
        if (_entries.ContainsKey(entry.Id))
        {
            throw new InvalidOperationException($"Duplicate content registration for '{entry.Id}'.");
        }

        _entries.Add(entry.Id, entry);
    }

    public bool TryGet(ContentId id, out T? entry) => _entries.TryGetValue(id, out entry);
}
