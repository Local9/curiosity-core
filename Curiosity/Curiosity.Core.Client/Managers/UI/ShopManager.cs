using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.Shop;
using System.Collections.Generic;

namespace Curiosity.Core.Client.Managers
{
    public class ShopManager : Manager<ShopManager>
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

                var categories = new List<dynamic>();

                foreach(ShopCategory shopCategory in result)
                {
                    var c = new
                    {
                        shopCategoryId = shopCategory.ShopCategoryID,
                        shopCategoryDescription = shopCategory.ShopCategoryDescription,
                        categories = new List<dynamic>()
                    };

                    foreach(ShopCategoryItem shopCategoryItem in shopCategory.Categories)
                    {
                        var i = new
                        {
                            shopCategoryItemId = shopCategoryItem.ShopCategoryItemID,
                            shopCategoryItemDescription = shopCategoryItem.ShopCategoryItemDescription
                        };

                        c.categories.Add(i);
                    }

                    categories.Add(c);
                }

                return categories;
            }));

            Instance.AttachNuiHandler("GetCategoryItems", new AsyncEventCallback(async metadata =>
            {
                List<CuriosityStoreItem> result = await EventSystem.Request<List<CuriosityStoreItem>>("shop:get:items", metadata.Find<int>(0));

                var items = new List<dynamic>();

                if (result is not null)
                {
                    foreach (CuriosityStoreItem storeItem in result)
                    {
                        var i = new
                        {
                            shopItemId = storeItem?.ShopItemId,
                            itemId = storeItem?.ItemId,
                            label = storeItem?.Label,
                            description = storeItem?.Description,
                            buyValue = storeItem?.BuyValue,
                            buyBackValue = storeItem?.BuyBackValue,
                            numberInStock = storeItem?.NumberInStock,
                            itemPurchased = storeItem?.ItemPurchased,
                            numberOwned = storeItem?.NumberOwned,
                            isStockManaged = storeItem?.IsStockManaged,
                            maximumAllowed = storeItem?.MaximumAllowed,
                            hasRoleRequirement = storeItem?.HasRoleRequirement,
                            numberOfSkillRequirements = storeItem?.NumberOfSkillRequirements,
                            numberOfItemRequirements = storeItem?.NumberOfItemRequirements,
                            imageUri = storeItem?.ImageUri
                        };

                        items.Add(i);
                    }
                }

                return items;
            }));

            Instance.AttachNuiHandler("ShopActions", new AsyncEventCallback(async metadata =>
            {
                bool shopAction = metadata.Find<bool>(0);
                int itemId = metadata.Find<int>(1);

                SqlResult result = await EventSystem.Request<SqlResult>("shop:item:action", shopAction, itemId);

                Notification notification = result.Success ? Notification.NOTIFICATION_SUCCESS : Notification.NOTIFICATION_INFO;

                NotificationManger.GetModule().SendNui(notification, result.Message, "bottom-right", "default");

                return new { success = result.Success };
            }));
        }
    }
}
