using System.IO;
using Godot;

namespace Downroot.Game.Infrastructure;

public sealed class PackPathResolver
{
    private readonly string _contentRoot;

    public PackPathResolver()
    {
        _contentRoot = ResolveContentRoot();
    }

    public string ResolveAbsolutePath(string packRelativePath)
    {
        var normalized = packRelativePath.Replace('/', Path.DirectorySeparatorChar);
        return Path.GetFullPath(Path.Combine(_contentRoot, normalized));
    }

    private static string ResolveContentRoot()
    {
        var configuredRoot = ProjectSettings.GetSetting("application/config/content_root").AsString();
        var projectDir = ProjectSettings.GlobalizePath("res://");

        var candidates = new List<string>();
        if (!string.IsNullOrWhiteSpace(configuredRoot))
        {
            candidates.Add(ProjectSettings.GlobalizePath(configuredRoot));
        }

        candidates.AddRange(GetCandidateAncestors(projectDir, 4));
        candidates.AddRange(GetCandidateAncestors(Directory.GetCurrentDirectory(), 6));
        candidates.AddRange(GetCandidateAncestors(AppContext.BaseDirectory, 6));

        foreach (var candidate in candidates
                     .Where(path => !string.IsNullOrWhiteSpace(path))
                     .Select(Path.GetFullPath)
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (Directory.Exists(Path.Combine(candidate, "packs", "basegame", "assets")))
            {
                return candidate;
            }
        }

        throw new DirectoryNotFoundException(
            $"Unable to locate content root. Checked configured root '{configuredRoot}', project dir '{projectDir}', current dir '{Directory.GetCurrentDirectory()}', and app base '{AppContext.BaseDirectory}'.");
    }

    private static IEnumerable<string> GetCandidateAncestors(string start, int depth)
    {
        var current = new DirectoryInfo(Path.GetFullPath(start));
        for (var index = 0; index <= depth && current is not null; index++)
        {
            yield return current.FullName;
            current = current.Parent;
        }
    }
}
