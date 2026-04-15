namespace Downroot.Core.Content;

public sealed class ResolvedContentPackSet
{
    public required IReadOnlyList<IContentPack> OrderedPacks { get; init; }
    public required IReadOnlyDictionary<string, IContentPack> ById { get; init; }
}
