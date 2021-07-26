using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.Shop;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Curiosity.Core.Client.Managers.UI
{
    public class InventoryManager : Manager<InventoryManager>
    {
        public override void Begin()
        {
            Instance.AttachNuiHandler("GetPlayerItems", new AsyncEventCallback(async metadata =>
            {
                List<CuriosityShopItem> lst = await EventSystem.Request<List<CuriosityShopItem>>("character:inventory:items");

                return JsonConvert.SerializeObject(lst);
            }));

            Instance.AttachNuiHandler("GetPlayerInventory", new AsyncEventCallback(async metadata =>
            {
                List<CuriosityShopItem> lst = await EventSystem.Request<List<CuriosityShopItem>>("character:inventory:equipped");

                return JsonConvert.SerializeObject(lst);
            }));
        }
    }
}
