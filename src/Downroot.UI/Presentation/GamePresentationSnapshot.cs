namespace Downroot.UI.Presentation;

public sealed record GamePresentationSnapshot(
    HudStatusViewData HudStatus,
    IReadOnlyList<HotbarSlotViewData> HotbarSlots,
    CraftingPanelViewData CraftingPanel,
    InteractionPromptViewData InteractionPrompt,
    StatusBannerViewData StatusBanner,
    DestroyProgressViewData DestroyProgress);
