using Downroot.Core.Ids;

namespace Downroot.Core.World;

public sealed record WorldGenPassDef(ContentId Id, string PassType, ContentId TerrainId);
