namespace Downroot.UI.Presentation;

public enum PromptIconKind
{
    Use,
    Open,
    Close,
    Gather,
    Eat,
    PickUp
}

public sealed record InteractionPromptViewData(
    bool IsVisible,
    string PromptKeyLabel,
    PromptIconKind PromptIconKind,
    string PromptVerbLabel,
    string PromptTargetLabel);
