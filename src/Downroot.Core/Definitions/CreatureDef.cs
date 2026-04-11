using Downroot.Core.Ids;

namespace Downroot.Core.Definitions;

public sealed record CreatureDef(
    ContentId Id,
    string DisplayName,
    string SourcePackId,
    string IdleSpriteSheetPath,
    string RunSpriteSheetPath,
    string? WorldSpritePath = null,
    int SpriteWidth = 64,
    int SpriteHeight = 64,
    float MoveSpeed = 0f,
    int ContactDamage = 0,
    bool NightOnlyAggro = false,
    int MaxHealth = 1) : ContentDef(Id, DisplayName, SourcePackId);
