using Downroot.Core.Ids;

namespace Downroot.Core.World;

public sealed record WorldGenPassDef(
    ContentId Id,
    string PassType,
    ContentId TargetId,
    int Count = 0,
    int StartColumn = 0,
    int StartRow = 0,
    int Width = 0,
    int Height = 0);
