using System.Linq;
using Atlas.Roleplay.Library.Inventory;

namespace Atlas.Roleplay.Client.Inventory.Impl
{
    public class EquipmentInventory : InventoryContainer
    {
        public EquipmentInventory(InventoryContainerBase inventoryBase) : base(inventoryBase)
        {
            Update += (item, type) =>
            {
                RefreshItemClassifications();

                if (type == InventoryUpdateType.Remove && item != null && item.IsWeapon())
                {
                    ((WeaponItem) item).Unequip();
                }
            };
        }

        public WeaponItem GetSelected()
        {
            return (WeaponItem) Slots?.FirstOrDefault(self =>
                self != null && self.IsWeapon() && ((WeaponItem) self).IsEquipped);
        }
    }
}