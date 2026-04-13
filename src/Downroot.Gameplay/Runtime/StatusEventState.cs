using Downroot.Core.Ids;

namespace Downroot.Gameplay.Runtime;

public sealed record StatusEventState(
    StatusEventKind Kind,
    ContentId? PrimaryContentId = null,
    int Amount = 0);
