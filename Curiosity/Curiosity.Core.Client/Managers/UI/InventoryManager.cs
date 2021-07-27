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

            Instance.AttachNuiHandler("AddToInventory", new AsyncEventCallback(async metadata =>
            {
                int itemId = -1;
                int numberOfItems = 1;

                if(!int.TryParse(metadata.Find<string>(0), out itemId))
                {
                    NotificationManger.GetModule().Error($"Item was invalid, please submit a ticket if you can replicate it.");
                    return new { success = false };
                }

                int.TryParse(metadata.Find<string>(1), out numberOfItems);

                ExportMessage result = await EventSystem.Request<ExportMessage>("character:inventory:equip", itemId, numberOfItems);

                if (!result.Success)
                {
                    NotificationManger.GetModule().Error($"{result.Error}");
                    return $"{result}";
                }

                return $"{result}";
            }));

            Instance.AttachNuiHandler("RemoveFromInventory", new AsyncEventCallback(async metadata =>
            {
                int itemId = -1;
                int numberOfItems = 1;

                if (!int.TryParse(metadata.Find<string>(0), out itemId))
                {
                    NotificationManger.GetModule().Error($"Item was invalid, please submit a ticket if you can replicate it.");
                    return new { success = false };
                }

                int.TryParse(metadata.Find<string>(1), out numberOfItems);

                ExportMessage result = await EventSystem.Request<ExportMessage>("character:inventory:remove", itemId, numberOfItems);

                if (!result.Success)
                {
                    NotificationManger.GetModule().Error($"{result.Error}");
                    return $"{result}";
                }

                return $"{result}";
            }));
        }
    }
}
