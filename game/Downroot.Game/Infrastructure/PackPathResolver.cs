using System.IO;
using Godot;

namespace Downroot.Game.Infrastructure;

public sealed class PackPathResolver
{
    private readonly string _repositoryRoot;

    public PackPathResolver()
    {
        var projectDir = ProjectSettings.GlobalizePath("res://");
        _repositoryRoot = Path.GetFullPath(Path.Combine(projectDir, "..", ".."));
    }

    public string ResolveAbsolutePath(string packRelativePath)
    {
        var normalized = packRelativePath.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(_repositoryRoot, normalized);
    }
}
