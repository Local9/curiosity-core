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
                List<CuriosityShopItem> result = await EventSystem.Request<List<CuriosityShopItem>>("shop:get:items", metadata.Find<int>(0));

                var items = new List<dynamic>();

                if (result is not null)
                {
                    foreach (CuriosityShopItem storeItem in result)
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

            Instance.AttachNuiHandler("GetItem", new AsyncEventCallback(async metadata =>
            {
                CuriosityShopItem storeItem = await EventSystem.Request<CuriosityShopItem>("shop:get:item", metadata.Find<int>(0));

                if (storeItem is not null)
                {
                    var item = new
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
                        imageUri = storeItem?.ImageUri,
                        skillsRequired = new List<dynamic>(),
                        itemsRequired = new List<dynamic>(),
                        rolesRequired = new List<dynamic>()
                    };

                    if (storeItem.SkillRequirements.Count > 0)
                    {
                        foreach (SkillRequirement skill in storeItem.SkillRequirements)
                        {
                            var s = new
                            {
                                label = skill.SkillLabel,
                                xpRequired = skill.ExperienceRequired,
                                xpCurrent = skill.ExperienceCurrent,
                                metRequirement = skill.RequirementMet
                            };
                            item.skillsRequired.Add(s);
                        }
                    }

                    if (storeItem.ItemRequirements.Count > 0)
                    {
                        foreach (ItemRequirement itemRequirement in storeItem.ItemRequirements)
                        {
                            var ir = new
                            {
                                label = itemRequirement.ItemLabel,
                                amountRequired = itemRequirement.AmountRequired,
                                amountCurrent = itemRequirement.AmountCurrent,
                                metRequirement = itemRequirement.RequirementMet
                            };
                            item.itemsRequired.Add(ir);
                        }
                    }

                    if (storeItem.RoleRequirements.Count > 0)
                    {
                        foreach (RoleRequirement role in storeItem.RoleRequirements)
                        {
                            var r = new
                            {
                                roleId = role.RoleId,
                                description = role.Description,
                                hasRole = role.HasRole
                            };
                            item.rolesRequired.Add(r);
                        }
                    }

                    return new { success = true, data = item };
                }
                return new { success = false };
            }));

            Instance.AttachNuiHandler("PurchaseItem", new AsyncEventCallback(async metadata =>
            {
                int itemId = metadata.Find<int>(0);

                SqlResult result = await EventSystem.Request<SqlResult>("shop:purchase:item", itemId);

                NotificationManger notificationManger = NotificationManger.GetModule();

                if (result.Success)
                {
                    notificationManger.Success(result.Message);
                }

                if (!result.Success)
                {
                    notificationManger.Warn(result.Message);
                }

                return new { success = result.Success, message = result.Message };
            }));

            Instance.AttachNuiHandler("SellItem", new AsyncEventCallback(async metadata =>
            {
                int itemId = metadata.Find<int>(0);

                SqlResult result = await EventSystem.Request<SqlResult>("shop:sell:item", itemId);

                NotificationManger notificationManger = NotificationManger.GetModule();

                if (result.Success)
                {
                    notificationManger.Success(result.Message);
                }

                if (!result.Success)
                {
                    notificationManger.Warn(result.Message);
                }

                return new { success = result.Success, message = result.Message };
            }));
        }
    }
}
