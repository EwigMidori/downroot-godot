using Downroot.Core.Ids;

namespace Downroot.Gameplay.Runtime;

public sealed class InventorySlot
{
    public ContentId? ItemId { get; private set; }
    public int Quantity { get; private set; }

    public bool IsEmpty => ItemId is null || Quantity <= 0;

    public void Set(ContentId itemId, int quantity)
    {
        ItemId = itemId;
        Quantity = quantity;
    }

    public void Clear()
    {
        ItemId = null;
        Quantity = 0;
    }

    public int Remove(int amount)
    {
        var removed = Math.Min(amount, Quantity);
        Quantity -= removed;
        if (Quantity <= 0)
        {
            Clear();
        }

        return removed;
    }
}
