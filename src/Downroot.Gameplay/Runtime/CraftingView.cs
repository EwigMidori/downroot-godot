using Downroot.Core.Definitions;

namespace Downroot.Gameplay.Runtime;

public sealed record CraftingView(IReadOnlyList<RecipeDef> AvailableRecipes);
