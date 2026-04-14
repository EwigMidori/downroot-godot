using Downroot.Content.Registries;
using Downroot.Core.Definitions;
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

    public InventoryState Clone()
    {
        var clone = new InventoryState(_slots.Count);
        for (var index = 0; index < _slots.Count; index++)
        {
            var slot = _slots[index];
            if (slot.ItemId is not null && slot.Quantity > 0)
            {
                clone._slots[index].Set(slot.ItemId.Value, slot.Quantity);
            }
        }

        return clone;
    }

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

    public bool CanAddMany(IEnumerable<ItemAmount> items, ContentRegistrySet registries)
    {
        var slots = _slots.Select(slot => new InventorySlotSnapshot(slot.ItemId, slot.Quantity)).ToList();

        foreach (var item in items)
        {
            if (!registries.Items.TryGet(item.ItemId, out var itemDef))
            {
                return false;
            }

            var remaining = item.Amount;
            var maxStack = itemDef!.MaxStack;

            foreach (var slot in slots.Where(slot => slot.ItemId == item.ItemId && slot.Quantity < maxStack))
            {
                var free = maxStack - slot.Quantity;
                var moved = Math.Min(free, remaining);
                slot.Quantity += moved;
                remaining -= moved;
                if (remaining <= 0)
                {
                    break;
                }
            }

            if (remaining > 0)
            {
                foreach (var slot in slots.Where(slot => slot.ItemId is null))
                {
                    var moved = Math.Min(maxStack, remaining);
                    slot.ItemId = item.ItemId;
                    slot.Quantity = moved;
                    remaining -= moved;
                    if (remaining <= 0)
                    {
                        break;
                    }
                }
            }

            if (remaining > 0)
            {
                return false;
            }
        }

        return true;
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

    public bool TryMoveSlotTo(int sourceIndex, InventoryState target, ContentRegistrySet registries)
    {
        if (sourceIndex < 0 || sourceIndex >= _slots.Count)
        {
            return false;
        }

        var source = _slots[sourceIndex];
        if (source.ItemId is null || source.Quantity <= 0)
        {
            return false;
        }

        if (!target.TryAdd(source.ItemId.Value, source.Quantity, registries))
        {
            return false;
        }

        source.Clear();
        return true;
    }

    private sealed class InventorySlotSnapshot(ContentId? itemId, int quantity)
    {
        public ContentId? ItemId { get; set; } = itemId;
        public int Quantity { get; set; } = quantity;
    }
}
