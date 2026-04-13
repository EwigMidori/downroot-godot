using Downroot.Core.Ids;

namespace Downroot.Gameplay.Runtime;

public sealed record InteractionContext(
    EntityId EntityId,
    WorldEntityKind EntityKind,
    ContentId ContentId,
    InteractionVerb Verb);
