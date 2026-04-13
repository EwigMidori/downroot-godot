using Downroot.Core.Ids;

namespace Downroot.Gameplay.Runtime;

public sealed class FurnaceTaskState(ContentId recipeId, EntityId furnaceEntityId, float durationSeconds)
{
    public ContentId RecipeId { get; } = recipeId;
    public EntityId FurnaceEntityId { get; } = furnaceEntityId;
    public float DurationSeconds { get; } = durationSeconds;
    public float ElapsedSeconds { get; set; }

    public float Progress01 => DurationSeconds <= 0f
        ? 1f
        : Math.Clamp(ElapsedSeconds / DurationSeconds, 0f, 1f);
}
