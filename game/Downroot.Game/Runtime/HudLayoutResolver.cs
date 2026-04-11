using Godot;

namespace Downroot.Game.Runtime;

public sealed class HudLayoutResolver
{
    private const float Margin = 16f;
    private const float Gap = 14f;
    private const float WorkspaceWidth = 364f;
    private const float WorkspaceHeight = 504f;
    private const float StatusMaxWidth = 336f;

    public void Apply(HudView view, Vector2 viewportSize)
    {
        view.HudRoot.Size = viewportSize;
        view.NightOverlay.Size = viewportSize;

        var rightColumnWidth = Math.Min(WorkspaceWidth, Math.Max(288f, viewportSize.X * 0.3f));
        LayoutTopLeft(view.PlayerStatusPanel, new Vector2(Margin, Margin), new Vector2(236, 92));
        LayoutRightColumn(view.CraftWorkspacePanel, viewportSize, rightColumnWidth);
        LayoutBottomCenter(view.HotbarPanel, viewportSize, view.HotbarPanel.Size);
        LayoutTopCenter(view.StatusBanner, viewportSize, new Vector2(Math.Min(StatusMaxWidth, viewportSize.X - 2 * Margin), view.StatusBanner.Size.Y));
        LayoutBottomLeft(view.PrimaryHelpPanel, viewportSize);
        LayoutPrompt(view, viewportSize);
    }

    public Vector2 ResolveDestroyPanelPosition(HudView view, Vector2 viewportSize, Vector2 targetScreenPosition)
    {
        var desired = targetScreenPosition + new Vector2(-view.DestroyProgressPanel.Size.X * 0.5f, -44f);
        desired.X = Mathf.Clamp(desired.X, Margin, Math.Max(Margin, viewportSize.X - view.DestroyProgressPanel.Size.X - Margin));
        desired.Y = Mathf.Clamp(desired.Y, Margin, Math.Max(Margin, viewportSize.Y - view.DestroyProgressPanel.Size.Y - Margin));

        var playerStatusBottom = view.PlayerStatusPanel.Position.Y + view.PlayerStatusPanel.Size.Y;
        if (RectOverlaps(desired, view.DestroyProgressPanel.Size, view.PlayerStatusPanel.Position, view.PlayerStatusPanel.Size))
        {
            desired.Y = playerStatusBottom + Gap;
        }

        if (RectOverlaps(desired, view.DestroyProgressPanel.Size, view.StatusBanner.Position, view.StatusBanner.Size))
        {
            desired.Y = view.StatusBanner.Position.Y + view.StatusBanner.Size.Y + Gap;
        }

        desired.Y = Mathf.Clamp(desired.Y, Margin, Math.Max(Margin, viewportSize.Y - view.DestroyProgressPanel.Size.Y - Margin));
        return desired;
    }

    private static void LayoutTopLeft(Control panel, Vector2 position, Vector2 size)
    {
        panel.Position = position;
        panel.Size = size;
    }

    private static void LayoutRightColumn(Control panel, Vector2 viewportSize, float width)
    {
        panel.Position = new Vector2(viewportSize.X - width - Margin, Margin);
        panel.Size = new Vector2(width, Math.Min(WorkspaceHeight, viewportSize.Y - 2 * Margin));
    }

    private static void LayoutBottomCenter(Control panel, Vector2 viewportSize, Vector2 size)
    {
        panel.Position = new Vector2((viewportSize.X - size.X) * 0.5f, viewportSize.Y - size.Y - Margin);
    }

    private static void LayoutTopCenter(Control panel, Vector2 viewportSize, Vector2 size)
    {
        panel.Size = size;
        panel.Position = new Vector2((viewportSize.X - size.X) * 0.5f, Margin);
    }

    private static void LayoutBottomLeft(Control panel, Vector2 viewportSize)
    {
        panel.Position = new Vector2(Margin, viewportSize.Y - panel.Size.Y - Margin);
    }

    private static void LayoutPrompt(HudView view, Vector2 viewportSize)
    {
        var hotbarTop = view.HotbarPanel.Position.Y;
        var desiredY = hotbarTop - view.ContextPromptPanel.Size.Y - Gap;
        var desiredX = (viewportSize.X - view.ContextPromptPanel.Size.X) * 0.5f;

        if (RectOverlaps(new Vector2(desiredX, desiredY), view.ContextPromptPanel.Size, view.CraftWorkspacePanel.Position, view.CraftWorkspacePanel.Size))
        {
            desiredX = view.CraftWorkspacePanel.Position.X - view.ContextPromptPanel.Size.X - Gap;
        }

        if (RectOverlaps(new Vector2(desiredX, desiredY), view.ContextPromptPanel.Size, view.PrimaryHelpPanel.Position, view.PrimaryHelpPanel.Size))
        {
            desiredY = view.PrimaryHelpPanel.Position.Y - view.ContextPromptPanel.Size.Y - Gap;
        }

        desiredX = Mathf.Clamp(desiredX, Margin, Math.Max(Margin, viewportSize.X - view.ContextPromptPanel.Size.X - Margin));
        desiredY = Mathf.Clamp(desiredY, Margin, Math.Max(Margin, viewportSize.Y - view.ContextPromptPanel.Size.Y - Margin));
        view.ContextPromptPanel.Position = new Vector2(desiredX, desiredY);
    }

    private static bool RectOverlaps(Vector2 aPos, Vector2 aSize, Vector2 bPos, Vector2 bSize)
    {
        return aPos.X < bPos.X + bSize.X
            && aPos.X + aSize.X > bPos.X
            && aPos.Y < bPos.Y + bSize.Y
            && aPos.Y + aSize.Y > bPos.Y;
    }
}
