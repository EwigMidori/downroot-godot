using Downroot.Core.Ids;

namespace Downroot.Core.Registries;

public interface IRegistry<T>
{
    IEnumerable<T> All { get; }
    int Count { get; }
    void Register(T entry);
    bool TryGet(ContentId id, out T? entry);
    T Get(ContentId id);
}
