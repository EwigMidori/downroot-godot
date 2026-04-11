using Downroot.Core.Ids;

namespace Downroot.Core.Definitions;

public abstract record ContentDef(ContentId Id, string DisplayName, string SourcePackId);
