namespace Downroot.Core.Content;

public sealed class ContentPackManifest
{
    public required string PackId { get; init; }
    public required string DisplayName { get; init; }
    public required string Version { get; init; }
    public required string Description { get; init; }
    public required bool IsBuiltIn { get; init; }
    public required IReadOnlyList<string> Dependencies { get; init; }
}
