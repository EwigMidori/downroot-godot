using Downroot.Core.Ids;

namespace Downroot.UI.Presentation;

public sealed record CraftRecipeViewData(
    ContentId RecipeId,
    ContentId ResultItemId,
    string RecipeName,
    IReadOnlyList<RecipeCostViewData> Costs,
    bool CanCraft);
