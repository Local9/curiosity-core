using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Web;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.Shop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Managers
{
    public class ShopManager : Manager<ShopManager>
    {
        /// <summary>
        /// GetCategories
        /// GetCategoryItems
        /// BuyItem
        /// SellItem
        /// </summary>

        public override void Begin()
        {
            EventSystem.GetModule().Attach("shop:get:stock", new AsyncEventCallback(async metadata =>
            {
                List<ShopStock> shopStocks = new List<ShopStock>();
                try
                {
                    shopStocks = await Database.Store.ShopDatabase.GetShopStock();
                    return shopStocks;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "shop:get:stock");
                    return shopStocks;
                }
            }));

            EventSystem.GetModule().Attach("shop:get:categories", new AsyncEventCallback(async metadata =>
            {
                try
                {
                    List<ShopCategory> categories = await Database.Store.ShopDatabase.GetCategories();
                    return categories;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "shop:get:categories");
                    return null;
                }
            }));

            EventSystem.GetModule().Attach("shop:get:items", new AsyncEventCallback(async metadata =>
            {
                try
                {
                    CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                    int categoryId = metadata.Find<int>(0);

                    return await Database.Store.ShopDatabase.GetCategoryItems(categoryId, curiosityUser.Character.CharacterId);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "shop:get:items");
                    return null;
                }
            }));

            EventSystem.GetModule().Attach("shop:get:item", new AsyncEventCallback(async metadata =>
            {
                int itemId = -1;
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                try
                {
                    itemId = metadata.Find<int>(0);

                    if (itemId == -1)
                    {
                        return null;
                    }

                    CuriosityShopItem item = await Database.Store.ShopDatabase.GetItem(itemId, curiosityUser.Character.CharacterId);
                    item.SkillRequirements = await Database.Store.ShopDatabase.GetSkillRequirements(itemId, curiosityUser.Character.CharacterId);
                    item.ItemRequirements = await Database.Store.ShopDatabase.GetItemRequirements(itemId, curiosityUser.Character.CharacterId);
                    item.RoleRequirements = await Database.Store.ShopDatabase.GetRoleRequirements(itemId, curiosityUser.Character.CharacterId);

                    return item;
                }
                catch (Exception ex)
                {
                    DiscordClient.GetModule().SendDiscordServerEventLogMessage($"shop:get:item: itemId {itemId}, characterId {curiosityUser.Character.CharacterId}\r{ex}");
                    await BaseScript.Delay(0);
                    Logger.Error(ex, "shop:get:items");
                    return null;
                }
            }));

            EventSystem.GetModule().Attach("shop:purchase:item", new AsyncEventCallback(async metadata =>
            {
                SqlResult sqlResult = new SqlResult();
                SqlResult purchaseResult = new SqlResult();
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                Player player = PluginManager.PlayersList[metadata.Sender];

                try
                {
                    if (curiosityUser.Purchasing)
                    {
                        sqlResult.Message = "Process Pending";
                        return sqlResult;
                    }

                    int itemId = metadata.Find<int>(0);
                    int characterId = curiosityUser.Character.CharacterId;

                    curiosityUser.Purchasing = true;

                    CuriosityShopItem item = new CuriosityShopItem();

                    goto GetItem;

                GetItem:
                    item = await Database.Store.ShopDatabase.GetItem(itemId, characterId);

                    if (item.IsStockManaged && item.NumberInStock == 0)
                        goto FailedPurchaseNotEnoughStock;

                    goto CheckWallet;

                CheckWallet:
                    // Check wallet
                    int characterCash = await Database.Store.BankDatabase.Get(characterId);
                    curiosityUser.Character.Cash = characterCash;

                    if (curiosityUser.Character.Cash < item.BuyValue) goto FailedPurchaseCash;
                    goto CheckRoles;

                CheckRoles:
                    // Check if item has roles
                    List<RoleRequirement> roleRequirements = await Database.Store.ShopDatabase.GetRoleRequirements(itemId, characterId);

                    if (roleRequirements.Count > 0)
                    {
                        Role role = curiosityUser.Role;

                        foreach (RoleRequirement roleRequirement in roleRequirements)
                        {
                            if (role == (Role)roleRequirement.RoleId)
                            {
                                goto CheckItems;
                            }
                        }

                        goto FailedPurchaseRole;
                    }

                    goto CheckItems; // Goto Item check is there are no role requirements to check

                CheckItems:
                    // check if item has item requirements
                    List<ItemRequirement> itemRequirements = await Database.Store.ShopDatabase.GetItemRequirements(itemId, characterId);

                    if (itemRequirements.Count > 0)
                    {
                        int requirements = itemRequirements.Count;
                        int metRequirements = 0;

                        foreach (ItemRequirement itemRequirement in itemRequirements)
                        {
                            if (itemRequirement.RequirementMet)
                                metRequirements++;
                        }

                        if (metRequirements == requirements) goto CheckSkills;

                        goto FailedPurchaseItem;
                    }

                    goto CheckSkills; // Goto skill check is there are no item requirements to check

                CheckSkills:
                    // check if item has skill requirements
                    List<SkillRequirement> skillRequirements = await Database.Store.ShopDatabase.GetSkillRequirements(itemId, characterId);

                    if (skillRequirements.Count > 0)
                    {
                        int requirements = skillRequirements.Count;
                        int metRequirements = 0;

                        foreach (SkillRequirement skillRequirement in skillRequirements)
                        {
                            if (skillRequirement.RequirementMet)
                                metRequirements++;
                        }

                        if (metRequirements == requirements)
                            goto ItemPurchaseRouter;

                        goto FailedPurchaseSkill;
                    }

                    goto ItemPurchaseRouter;

                ItemPurchaseRouter:
                    if (item.IsVehicle || item.SpawnTypeId == SpawnType.Trailer)
                        goto AddVehicleToCharacter;

                    goto AddItemToCharacter;

                AddVehicleToCharacter:
                    purchaseResult = await Database.Store.ShopDatabase.PurchaseVehicle(itemId, characterId);
                    if (purchaseResult.Success) goto AdjustWallet;
                    goto FailedPurchase;

                AddItemToCharacter:
                    // at this point the item type will be important
                    purchaseResult = await Database.Store.ShopDatabase.PurchaseItem(itemId, characterId);
                    if (purchaseResult.Success) goto AdjustWallet;
                    goto FailedPurchase;

                AdjustWallet:
                    // if all pass, allow purchase, else fail
                    int minusValue = (int)item.BuyValue * -1;
                    int newWalletValue = await Database.Store.BankDatabase.Adjust(characterId, minusValue);
                    curiosityUser.Character.Cash = newWalletValue;

                    if (item.IsStockManaged)
                        goto LowerStockAmount;

                    goto SuccessfulPurchase;

                LowerStockAmount:
                    if (item.IsStockManaged)
                        await Database.Store.ShopDatabase.Adjust((int)item.ShopItemId, -1);

                    goto SuccessfulPurchase;

                SuccessfulPurchase:
                    curiosityUser.Purchasing = false;
                    sqlResult.Success = true;
                    sqlResult.Message = purchaseResult.Message;

                    Web.DiscordClient discordClient = Web.DiscordClient.GetModule();
                    discordClient.SendDiscordPlayerLogMessage($"Player '{curiosityUser.LatestName}' purchased '{item.Label}' for ${item.BuyValue}"); // MOVE TO DB LOG
                    await BaseScript.Delay(0);

                    if (item.IsVehicle)
                    {
                        EventSystem.Send("garage:update", metadata.Sender, item.Label);
                    }

                    if (item.IsWeapon)
                    {
                        int hash = API.GetHashKey(item.HashKey);
                        await BaseScript.Delay(0);
                        API.GiveWeaponToPed(player.Character.Handle, (uint)hash, 999, false, false);
                        await Database.Store.CharacterDatabase.InsertInventoryItem(curiosityUser.Character.CharacterId, item.ItemId, 1);
                    }

                    goto ReturnResult;

                FailedPurchase:
                    sqlResult.Message = purchaseResult.Message;
                    goto ReturnResult; // Need to write an error logger

                FailedPurchaseNotEnoughStock:
                    sqlResult.Message = "Not enough items in stock";
                    goto ReturnResult;

                FailedPurchaseCash:
                    sqlResult.Message = "Not enough cash to buy item";
                    goto ReturnResult;

                FailedPurchaseRole:
                    sqlResult.Message = "Role Requirement not met";
                    goto ReturnResult;

                FailedPurchaseItem:
                    sqlResult.Message = "Item Requirement not met";
                    goto ReturnResult;

                FailedPurchaseSkill:
                    sqlResult.Message = "Skill Requirement not met";
                    goto ReturnResult;

                ReturnResult:
                    curiosityUser.Purchasing = false;
                    return sqlResult;

                }
                catch (Exception ex)
                {
                    DiscordClient.GetModule().SendDiscordServerEventLogMessage($"[ERROR] shop:purchase:item\r{ex}");
                    await BaseScript.Delay(0);

                    Logger.Error(ex, "shop:purchase:item");
                    curiosityUser.Purchasing = false;
                    return sqlResult;
                }
            }));

            EventSystem.GetModule().Attach("shop:sell:item", new AsyncEventCallback(async metadata =>
            {
                SqlResult sqlResult = new SqlResult();
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                try
                {

                    if (curiosityUser.Purchasing)
                    {
                        sqlResult.Message = "Process Pending";
                        return sqlResult;
                    }

                    int itemId = metadata.Find<int>(0);
                    int characterId = curiosityUser.Character.CharacterId;

                    curiosityUser.Purchasing = true;

                    CuriosityShopItem item = new CuriosityShopItem();

                    goto GetItem;

                GetItem:
                    item = await Database.Store.ShopDatabase.GetItem(itemId, characterId);

                    if (item is null)
                        goto Fail;

                    goto CheckUserOwnsItem;

                CheckUserOwnsItem:
                    CuriosityShopItem ownedItem = await Database.Store.CharacterDatabase.GetItem(characterId, itemId);

                    if (ownedItem is null)
                        goto FailItemNotOwned;

                    if (ownedItem.ShopItemId is null)
                        goto UnableToSellItem;

                    if (ownedItem.NumberOwned == 0)
                        goto FailItemNotOwned;

                    goto RemoveItem;

                RemoveItem:
                    bool itemRemoved = await Database.Store.CharacterDatabase.RemoveItem(characterId, itemId);

                    if (itemRemoved)
                        goto PayBackUser;

                    if (!itemRemoved)
                        goto Fail;

                PayBackUser:
                    curiosityUser.Character.Cash = await Database.Store.BankDatabase.Adjust(characterId, (int)ownedItem.BuyBackValue);

                    if (item.IsStockManaged)
                        goto AdjustStock;

                    goto Success;

                AdjustStock:
                    await Database.Store.ShopDatabase.Adjust((int)item.ShopItemId, 1);

                    goto Success;

                Success:
                    sqlResult.Success = true;
                    sqlResult.Message = "Item sold";
                    goto ReturnResult;

                UnableToSellItem:
                    sqlResult.Message = "Item unable to be sold";
                    goto ReturnResult;

                FailItemNotOwned:
                    sqlResult.Message = "Item not owned.";
                    goto ReturnResult;

                Fail:
                    goto ReturnResult;

                ReturnResult:
                    curiosityUser.Purchasing = false;
                    return sqlResult;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "shop:sell:item");
                    curiosityUser.Purchasing = false;
                    return sqlResult;
                }
            }));

            Instance.ExportDictionary.Add("StockAdjust", new Func<int, int, Task<string>>(
                async (shopItemId, amount) =>
                {
                    ExportMessage exportMessage = new ExportMessage();

                    bool success = await Database.Store.ShopDatabase.Adjust(shopItemId, amount);

                    if (!success)
                    {
                        exportMessage.error = "No rows affected";
                    }

                    return $"{exportMessage}";
                }));

            Instance.ExportDictionary.Add("Stock", new Func<string, int, Task<string>>(
                async (playerHandle, itemId) =>
                {
                    ExportMessage exportMessage = new ExportMessage();

                    int playerId = 0;
                    if (!int.TryParse(playerHandle, out playerId))
                        exportMessage.error = "First parameter is not a number";

                    if (!PluginManager.ActiveUsers.ContainsKey(playerId))
                        exportMessage.error = "Player was not found";

                    CuriosityUser user = PluginManager.ActiveUsers[playerId];

                    CuriosityShopItem item = await Database.Store.ShopDatabase.GetItem(itemId, user.Character.CharacterId);

                    if (item is null)
                    {
                        exportMessage.error = "No item found";
                    }
                    else
                    {
                        exportMessage.newNumberValue = item.NumberInStock;
                    }

                    return $"{exportMessage}";
                }));
        }
    }
}
