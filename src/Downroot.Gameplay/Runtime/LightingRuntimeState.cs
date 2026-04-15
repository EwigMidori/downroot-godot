using Downroot.Core.World;

namespace Downroot.Gameplay.Runtime;

public sealed class LightingRuntimeState
{
    public LightingField? Field { get; private set; }
    public LightingFieldBounds Bounds { get; private set; } = new(0, 0, 1, 1);
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
    public LightingFieldBounds? ValueDirtyBounds { get; private set; }

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
        ValueDirtyBounds = null;
    }

    public void MarkValueDirty(LightingFieldBounds? dirtyBounds = null)
    {
        ValueVersion++;
        IsValueDirty = true;
        if (dirtyBounds is null)
        {
            ValueDirtyBounds = null;
            return;
        }

        ValueDirtyBounds = ValueDirtyBounds is { } existing
            ? LightingFieldBounds.Union(existing, dirtyBounds.Value)
            : dirtyBounds.Value;
    }

    public void UpdateSkylightBucket(int bucket)
    {
        if (SkylightBucket == bucket)
        {
            return;
        }

        SkylightBucket = bucket;
        MarkValueDirty(Bounds);
    }

    public void UpdateInputs(
        LightingFieldBounds bounds,
        IReadOnlyList<RuntimeLightEmitter> emitters,
        IReadOnlyList<RuntimeLightOccluder> occluders,
        IReadOnlyList<RuntimeSkylightMask> skylightMasks)
    {
        Bounds = bounds;
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
        ValueDirtyBounds = null;
    }
}
