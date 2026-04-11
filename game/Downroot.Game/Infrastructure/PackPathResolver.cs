using System.IO;
using Godot;

namespace Downroot.Game.Infrastructure;

public sealed class PackPathResolver
{
    private readonly string _contentRoot;

    public PackPathResolver()
    {
        var configuredRoot = ProjectSettings.GetSetting("application/config/content_root").AsString();
        if (string.IsNullOrWhiteSpace(configuredRoot))
        {
            throw new InvalidOperationException("Missing Godot setting 'application/config/content_root'.");
        }

        _contentRoot = ProjectSettings.GlobalizePath(configuredRoot);
    }

    public string ResolveAbsolutePath(string packRelativePath)
    {
        var normalized = packRelativePath.Replace('/', Path.DirectorySeparatorChar);
        return Path.GetFullPath(Path.Combine(_contentRoot, normalized));
    }
}
