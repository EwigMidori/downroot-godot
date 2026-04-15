using Downroot.Core.Content;

namespace Downroot.Content.Packs;

public sealed class ContentPackLocator
{
    public IReadOnlyList<IContentPack> LocatePacks() => [new BaseGameContentPack(), new PortalModContentPack()];
}
