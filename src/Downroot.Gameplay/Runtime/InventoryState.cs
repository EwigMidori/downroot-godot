using Downroot.Content.Registries;
using Downroot.Core.Ids;

namespace Downroot.Gameplay.Runtime;

public sealed class InventoryState
{
    private readonly List<InventorySlot> _slots;

    public InventoryState(int slotCount)
    {
        _slots = Enumerable.Range(0, slotCount).Select(_ => new InventorySlot()).ToList();
    }

    public IReadOnlyList<InventorySlot> Slots => _slots;

    public bool TryAdd(ContentId itemId, int quantity, ContentRegistrySet registries)
    {
        if (!registries.Items.TryGet(itemId, out var itemDef))
        {
            return false;
        }

        var resolvedItemDef = itemDef!;

        var remaining = quantity;
        foreach (var slot in _slots.Where(slot => slot.ItemId == itemId && slot.Quantity < resolvedItemDef.MaxStack))
        {
            var free = resolvedItemDef.MaxStack - slot.Quantity;
            var moved = Math.Min(free, remaining);
            slot.Set(itemId, slot.Quantity + moved);
            remaining -= moved;
            if (remaining <= 0)
            {
                return true;
            }
        }

        foreach (var slot in _slots.Where(slot => slot.IsEmpty))
        {
            var moved = Math.Min(resolvedItemDef.MaxStack, remaining);
            slot.Set(itemId, moved);
            remaining -= moved;
            if (remaining <= 0)
            {
                return true;
            }
        }

        return remaining == 0;
    }

    public bool CanAdd(ContentId itemId, int quantity, ContentRegistrySet registries)
    {
        if (!registries.Items.TryGet(itemId, out var itemDef))
        {
            return false;
        }

        var resolvedItemDef = itemDef!;
        var remaining = quantity;
        foreach (var slot in _slots.Where(slot => slot.ItemId == itemId && slot.Quantity < resolvedItemDef.MaxStack))
        {
            var free = resolvedItemDef.MaxStack - slot.Quantity;
            remaining -= Math.Min(free, remaining);
            if (remaining <= 0)
            {
                return true;
            }
        }

        foreach (var slot in _slots.Where(slot => slot.IsEmpty))
        {
            remaining -= Math.Min(resolvedItemDef.MaxStack, remaining);
            if (remaining <= 0)
            {
                return true;
            }
        }

        return remaining <= 0;
    }

    public bool Has(ContentId itemId, int quantity)
    {
        return _slots.Where(slot => slot.ItemId == itemId).Sum(slot => slot.Quantity) >= quantity;
    }

    public int Count(ContentId itemId)
    {
        return _slots.Where(slot => slot.ItemId == itemId).Sum(slot => slot.Quantity);
    }

    public bool TryConsume(ContentId itemId, int quantity)
    {
        if (!Has(itemId, quantity))
        {
            return false;
        }

        var remaining = quantity;
        foreach (var slot in _slots.Where(slot => slot.ItemId == itemId && !slot.IsEmpty))
        {
            remaining -= slot.Remove(remaining);
            if (remaining <= 0)
            {
                return true;
            }
        }

        return true;
    }
}
