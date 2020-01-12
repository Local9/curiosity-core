using System;
using System.Linq;
using Curiosity.System.Client.Inventory.Items;
using Curiosity.System.Library.Inventory;

namespace Curiosity.System.Client.Inventory
{
    public class ItemHelper
    {
        public static bool Give(InventoryContainer container, InventoryItem item, int slot = -1)
        {
            if (slot == -1)
            {
                if (container.Slots.All(self => self != null && self.Seed != "__none")) return false;

                var index = -1;

                foreach (var entry in container.Slots)
                {
                    index++;

                    if (entry != null && entry.Seed != "__none") continue;

                    item.Slot = index;

                    break;
                }
            }
            else
            {
                item.Slot = slot;
            }

            try
            {
                container.Slots[item.Slot] = item;
                container.Commit();

                Cache.Player.Sound.PlayFrontend(item.IsWeapon() ? "PICK_UP_WEAPON" : "PICK_UP",
                    item.IsWeapon() ? "HUD_FRONTEND_CUSTOM_SOUNDSET" : "HUD_FRONTEND_DEFAULT_SOUNDSET");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void Remove(InventoryItem item)
        {
            var container = item.GetParent();

            container.Slots[item.Slot] = new EmptyItem();
            container.Commit();
        }

        public static void RemoveAll(InventoryContainer container, Predicate<InventoryItem> predicate)
        {
            container.Slots.Where(self => self != null).Where(predicate.Invoke).ToList()
                .ForEach(self => container.Slots[self.Slot] = null);
            container.Commit();
        }
    }
}