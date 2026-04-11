using Godot;

namespace Downroot.Game.Runtime;

public sealed class HudLayoutResolver
{
    private const float Margin = 16f;
    private const float Gap = 14f;
    private const float WorkspaceMinWidth = 320f;
    private const float WorkspaceMaxWidth = 364f;
    private const float WorkspaceMaxHeight = 504f;
    private const float StatusMaxWidth = 336f;

    public void Apply(HudView view, Vector2 viewportSize)
    {
        view.HudRoot.Size = viewportSize;
        view.NightOverlay.Size = viewportSize;

        var playerStatusSize = MaxSize(view.PlayerStatusPanel.GetCombinedMinimumSize(), new Vector2(236, 92));
        var hotbarSize = MaxSize(view.HotbarPanel.GetCombinedMinimumSize(), new Vector2(504, 72));
        var helpSize = MaxSize(view.PrimaryHelpPanel.GetCombinedMinimumSize(), new Vector2(420, 68));
        var statusSize = MaxSize(view.StatusBanner.GetCombinedMinimumSize(), new Vector2(220, 40));
        var promptSize = MaxSize(view.ContextPromptPanel.GetCombinedMinimumSize(), new Vector2(260, 40));
        var workspaceWidth = Mathf.Clamp(viewportSize.X * 0.3f, WorkspaceMinWidth, WorkspaceMaxWidth);
        var workspaceHeight = Math.Min(WorkspaceMaxHeight, viewportSize.Y - 2 * Margin);
        var workspaceSize = view.CraftWorkspacePanel.Visible
            ? MaxSize(view.CraftWorkspacePanel.GetCombinedMinimumSize(), new Vector2(workspaceWidth, workspaceHeight))
            : new Vector2(workspaceWidth, workspaceHeight);

        LayoutTopLeft(view.PlayerStatusPanel, new Vector2(Margin, Margin), playerStatusSize);
        LayoutRightColumn(view.CraftWorkspacePanel, viewportSize, workspaceSize);
        LayoutTopCenter(view.StatusBanner, viewportSize, new Vector2(Math.Min(StatusMaxWidth, viewportSize.X - 2 * Margin), statusSize.Y));
        LayoutHotbar(view, viewportSize, hotbarSize);
        LayoutHelp(view, viewportSize, helpSize);
        LayoutPrompt(view, viewportSize, promptSize);
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

    private static void LayoutRightColumn(Control panel, Vector2 viewportSize, Vector2 size)
    {
        panel.Position = new Vector2(viewportSize.X - size.X - Margin, Margin);
        panel.Size = size;
    }

    private static void LayoutTopCenter(Control panel, Vector2 viewportSize, Vector2 size)
    {
        panel.Size = size;
        panel.Position = new Vector2((viewportSize.X - size.X) * 0.5f, Margin);
    }

    private static void LayoutHotbar(HudView view, Vector2 viewportSize, Vector2 size)
    {
        var hotbarY = viewportSize.Y - size.Y - Margin;
        var leftOccupiedRight = view.PrimaryHelpPanel.Position.X + view.PrimaryHelpPanel.Size.X + Gap;
        var rightOccupiedLeft = view.CraftWorkspacePanel.Visible
            ? view.CraftWorkspacePanel.Position.X - Gap
            : viewportSize.X - Margin;
        var laneLeft = Math.Max(Margin, leftOccupiedRight);
        var laneRight = Math.Max(laneLeft, rightOccupiedLeft);
        var laneWidth = laneRight - laneLeft;

        if (size.X <= laneWidth)
        {
            var centeredX = laneLeft + (laneWidth - size.X) * 0.5f;
            view.HotbarPanel.Size = size;
            view.HotbarPanel.Position = new Vector2(centeredX, hotbarY);
            return;
        }

        var fallbackWidth = Math.Max(200f, laneWidth);
        view.HotbarPanel.Size = new Vector2(fallbackWidth, size.Y);
        view.HotbarPanel.Position = new Vector2(laneLeft, hotbarY);
    }

    private static void LayoutHelp(HudView view, Vector2 viewportSize, Vector2 size)
    {
        var desired = new Vector2(Margin, viewportSize.Y - size.Y - Margin);
        if (RectOverlaps(desired, size, view.HotbarPanel.Position, view.HotbarPanel.Size))
        {
            desired.Y = view.HotbarPanel.Position.Y - size.Y - Gap;
        }

        desired.Y = Mathf.Clamp(desired.Y, Margin, Math.Max(Margin, viewportSize.Y - size.Y - Margin));
        view.PrimaryHelpPanel.Size = size;
        view.PrimaryHelpPanel.Position = desired;
    }

    private static void LayoutPrompt(HudView view, Vector2 viewportSize, Vector2 size)
    {
        var hotbarTop = view.HotbarPanel.Position.Y;
        var leftBound = Math.Max(Margin, view.PrimaryHelpPanel.Position.X + view.PrimaryHelpPanel.Size.X + Gap);
        var rightBound = view.CraftWorkspacePanel.Visible
            ? view.CraftWorkspacePanel.Position.X - Gap
            : viewportSize.X - Margin;
        var availableWidth = rightBound - leftBound;
        var desiredX = leftBound + Math.Max(0f, (availableWidth - size.X) * 0.5f);
        var desiredY = hotbarTop - size.Y - Gap;

        if (RectOverlaps(new Vector2(desiredX, desiredY), size, view.PrimaryHelpPanel.Position, view.PrimaryHelpPanel.Size))
        {
            desiredY = view.PrimaryHelpPanel.Position.Y - size.Y - Gap;
        }

        if (RectOverlaps(new Vector2(desiredX, desiredY), size, view.StatusBanner.Position, view.StatusBanner.Size))
        {
            desiredY = view.StatusBanner.Position.Y + view.StatusBanner.Size.Y + Gap;
        }

        desiredX = Mathf.Clamp(desiredX, leftBound, Math.Max(leftBound, rightBound - size.X));
        desiredY = Mathf.Clamp(desiredY, Margin, Math.Max(Margin, viewportSize.Y - size.Y - Margin));
        view.ContextPromptPanel.Size = size;
        view.ContextPromptPanel.Position = new Vector2(desiredX, desiredY);
    }

    private static Vector2 MaxSize(Vector2 value, Vector2 minimum)
    {
        return new Vector2(Math.Max(value.X, minimum.X), Math.Max(value.Y, minimum.Y));
    }

    private static bool RectOverlaps(Vector2 aPos, Vector2 aSize, Vector2 bPos, Vector2 bSize)
    {
        return aPos.X < bPos.X + bSize.X
            && aPos.X + aSize.X > bPos.X
            && aPos.Y < bPos.Y + bSize.Y
            && aPos.Y + aSize.Y > bPos.Y;
    }
}
