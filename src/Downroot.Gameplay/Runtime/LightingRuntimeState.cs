using Downroot.Core.World;

namespace Downroot.Gameplay.Runtime;

public sealed class LightingRuntimeState
{
    public LightingField? Field { get; private set; }
    public IReadOnlyList<RuntimeLightEmitter> Emitters { get; private set; } = [];
    public IReadOnlyList<RuntimeLightOccluder> Occluders { get; private set; } = [];
    public IReadOnlyList<RuntimeSkylightMask> SkylightMasks { get; private set; } = [];
    public long StructureVersion { get; private set; }
    public long ValueVersion { get; private set; }
    public long FieldVersion { get; private set; }
    public bool IsStructureDirty { get; private set; } = true;
    public bool IsValueDirty { get; private set; } = true;
    public int SkylightBucket { get; private set; } = -1;
    public WorldSpaceKind ActiveWorldSpaceKind { get; private set; } = WorldSpaceKind.Overworld;

    public void SetActiveWorld(WorldSpaceKind worldSpaceKind)
    {
        if (ActiveWorldSpaceKind == worldSpaceKind)
        {
            return;
        }

        ActiveWorldSpaceKind = worldSpaceKind;
        MarkStructureDirty();
    }

    public void MarkStructureDirty()
    {
        StructureVersion++;
        IsStructureDirty = true;
        IsValueDirty = true;
    }

    public void MarkValueDirty()
    {
        ValueVersion++;
        IsValueDirty = true;
    }

    public void UpdateSkylightBucket(int bucket)
    {
        if (SkylightBucket == bucket)
        {
            return;
        }

        SkylightBucket = bucket;
        MarkValueDirty();
    }

    public void UpdateInputs(
        IReadOnlyList<RuntimeLightEmitter> emitters,
        IReadOnlyList<RuntimeLightOccluder> occluders,
        IReadOnlyList<RuntimeSkylightMask> skylightMasks)
    {
        Emitters = emitters;
        Occluders = occluders;
        SkylightMasks = skylightMasks;
    }

    public void ApplyField(LightingField field)
    {
        Field = field;
        FieldVersion++;
        IsStructureDirty = false;
        IsValueDirty = false;
    }
}
