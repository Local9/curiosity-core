using Atlas.Roleplay.Library.Inventory;
using System.Linq;

namespace Atlas.Roleplay.Client.Inventory
{
    public static class InventoryItemExtensions
    {
        public static InventoryContainer GetParent(this InventoryItem source)
        {
            return InventoryManager.GetModule().Registry.FirstOrDefault(self =>
                self.Slots.Any(item => item != null && item.Seed == source.Seed));
        }
    }
}