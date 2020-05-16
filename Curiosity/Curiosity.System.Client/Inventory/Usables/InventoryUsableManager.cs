using Curiosity.System.Client.Managers;
using Curiosity.System.Library.Events;
using Curiosity.System.Library.Inventory;
using Newtonsoft.Json;

namespace Curiosity.System.Client.Inventory.Usables
{
    public class InventoryUsableManager : Manager<InventoryUsableManager>
    {
        public override void Begin()
        {
            Atlas.AttachNuiHandler("INVENTORY_ITEM_USE", new EventCallback(metadata =>
            {
                var item = JsonConvert.DeserializeObject<InventoryItem>(metadata.Find<string>(0));

                // More complex solution to this later, like registering through a class or something like that rather than having everything here.
                if (item.Name == "bandage")
                {
                    Cache.Entity.Health += 75;
                    Cache.Player.Sound.PlayFrontend("Bus_Schedule_Pickup", "DLC_PRISON_BREAK_HEIST_SOUNDS");

                    ItemHelper.Remove(item);
                }

                return null;
            }));
        }
    }
}