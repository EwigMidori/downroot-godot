using Downroot.Core.Ids;
using Downroot.Core.Save;
using Downroot.Gameplay.Runtime;

namespace Downroot.Gameplay.Persistence;

public sealed class InventoryPersistenceAdapter
{
    public IReadOnlyList<SavedInventorySlotData> Export(InventoryState inventory)
    {
        return inventory.Slots
            .Select((slot, index) => new SavedInventorySlotData
            {
                SlotIndex = index,
                ItemId = slot.ItemId?.Value,
                Quantity = slot.Quantity
            })
            .ToArray();
    }

    public void Import(InventoryState inventory, IEnumerable<SavedInventorySlotData> savedSlots)
    {
        for (var index = 0; index < inventory.SlotCount; index++)
        {
            inventory.SetSlot(index, null, 0);
        }

        foreach (var slot in savedSlots)
        {
            inventory.SetSlot(slot.SlotIndex, slot.ItemId is null ? null : new ContentId(slot.ItemId), slot.Quantity);
        }
    }
}
