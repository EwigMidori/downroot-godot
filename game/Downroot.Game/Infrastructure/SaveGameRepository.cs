using System.Text.RegularExpressions;
using Downroot.Core.Save;

namespace Downroot.Game.Infrastructure;

public sealed class SaveGameRepository
{
    private static readonly Regex SlotSlugRegex = new("[^a-z0-9-]+", RegexOptions.Compiled);

    private readonly SavePathResolver _paths;
    private readonly JsonFileStore _store;

    public SaveGameRepository(SavePathResolver paths, JsonFileStore store)
    {
        _paths = paths;
        _store = store;
    }

    public SaveManifest LoadManifest()
    {
        return _store.Read<SaveManifest>(_paths.GetManifestPath()) ?? new SaveManifest();
    }

    public void SaveManifest(SaveManifest manifest)
    {
        _store.Write(_paths.GetManifestPath(), manifest);
    }

    public SaveGameData? LoadSave(string slotId)
    {
        return _store.Read<SaveGameData>(_paths.GetSavePath(slotId));
    }

    public IReadOnlyList<SaveSlotSummary> ListSlots()
    {
        return LoadManifest().Slots
            .OrderByDescending(slot => slot.LastWriteUtc)
            .ToArray();
    }

    public void SaveGame(SaveGameData save)
    {
        _store.Write(_paths.GetSavePath(save.SlotId), save);
        var manifest = LoadManifest();
        var slots = manifest.Slots.ToDictionary(slot => slot.SlotId, StringComparer.Ordinal);
        slots[save.SlotId] = new SaveSlotSummary
        {
            SlotId = save.SlotId,
            DisplayName = save.DisplayName,
            WorldSeed = save.WorldSeed,
            LastWriteUtc = DateTimeOffset.UtcNow,
            CurrentWorldSpace = save.ActiveWorldSpaceKind,
            PlayerHealth = save.Player.Health,
            PlayerHunger = save.Player.Hunger
        };

        SaveManifest(new SaveManifest
        {
            LastPlayedSlotId = save.SlotId,
            Slots = slots.Values.OrderByDescending(slot => slot.LastWriteUtc).ToArray()
        });
    }

    public void DeleteSave(string slotId)
    {
        var path = _paths.Globalize(_paths.GetSavePath(slotId));
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }

        var manifest = LoadManifest();
        SaveManifest(new SaveManifest
        {
            LastPlayedSlotId = manifest.LastPlayedSlotId == slotId ? null : manifest.LastPlayedSlotId,
            Slots = manifest.Slots.Where(slot => !string.Equals(slot.SlotId, slotId, StringComparison.Ordinal)).ToArray()
        });
    }

    public void SetLastPlayedSlot(string slotId)
    {
        var manifest = LoadManifest();
        manifest.LastPlayedSlotId = slotId;
        SaveManifest(manifest);
    }

    public string CreateSlotId(string displayName)
    {
        var baseSlug = SlotSlugRegex.Replace(displayName.Trim().ToLowerInvariant().Replace(' ', '-'), "-").Trim('-');
        if (string.IsNullOrWhiteSpace(baseSlug))
        {
            baseSlug = "save";
        }

        var existing = LoadManifest().Slots.Select(slot => slot.SlotId).ToHashSet(StringComparer.Ordinal);
        if (!existing.Contains(baseSlug))
        {
            return baseSlug;
        }

        for (var suffix = 2; suffix < 10_000; suffix++)
        {
            var candidate = $"{baseSlug}-{suffix}";
            if (!existing.Contains(candidate))
            {
                return candidate;
            }
        }

        return $"{baseSlug}-{Guid.NewGuid():N}"[..Math.Min(baseSlug.Length + 9, 24)];
    }
}
