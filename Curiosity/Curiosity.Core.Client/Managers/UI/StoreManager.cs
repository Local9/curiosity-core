using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.Shop;
using System.Collections.Generic;

namespace Curiosity.Core.Client.Managers
{
    public class StoreManager : Manager<StoreManager>
    {
        public override void Begin()
        {

            Instance.AttachNuiHandler("VehicleStoreAction", new EventCallback(metadata =>
            {
                // BaseScript.TriggerServerEvent("curiosity:Server:Vehicle:Shop:Action", metadata.Find<int>(0));
                return null;
            }));

            // New Shop Methods
            Instance.AttachNuiHandler("GetShopCategories", new AsyncEventCallback(async metadata =>
            {
                List<ShopCategory> result = await EventSystem.Request<List<ShopCategory>>("shop:get:categories");

                string jsn = new JsonBuilder()
                    .Add("operation", "SHOP_CATEGORIES")
                    .Add("list", result)
                    .Build();

                API.SendNuiMessage(jsn);

                return null;
            }));

            Instance.AttachNuiHandler("GetCategoryItems", new AsyncEventCallback(async metadata =>
            {
                List<CuriosityStoreItem> result = await EventSystem.Request<List<CuriosityStoreItem>>("shop:get:items", metadata.Find<int>(0));

                string jsn = new JsonBuilder()
                    .Add("operation", "SHOP_CATEGORY_ITEMS")
                    .Add("list", result)
                    .Build();

                API.SendNuiMessage(jsn);

                return null;
            }));

            Instance.AttachNuiHandler("ShopActions", new AsyncEventCallback(async metadata =>
            {
                bool shopAction = metadata.Find<bool>(0);
                int itemId = metadata.Find<int>(1);

                SqlResult result = await EventSystem.Request<SqlResult>("shop:item:action", shopAction, itemId);

                string jsn = new JsonBuilder()
                    .Add("operation", "SHOP_PURCHASE_CONFIRM")
                    .Build();

                API.SendNuiMessage(jsn);

                Notification notification = result.Success ? Notification.NOTIFICATION_SUCCESS : Notification.NOTIFICATION_INFO;

                NotificationManger.GetModule().SendNui(notification, result.Message, "bottom-right", "default");

                return null;
            }));
        }
    }
}
