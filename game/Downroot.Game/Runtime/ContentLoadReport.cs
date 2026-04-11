using Godot;

namespace Downroot.Game.Runtime;

public sealed class ContentLoadReport
{
    private readonly List<string> _lines = [];

    public void AddSuccess(string contentId, string absolutePath)
    {
        var line = $"OK {contentId} <= {absolutePath}";
        _lines.Add(line);
        GD.Print(line);
    }

    public IReadOnlyList<string> Lines => _lines;

    public string ToDisplayText()
    {
        return string.Join('\n', _lines);
    }
}
