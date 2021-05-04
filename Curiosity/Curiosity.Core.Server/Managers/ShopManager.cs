using CitizenFX.Core;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.Shop;
using System;
using System.Collections.Generic;

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
                try
                {
                    CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                    int itemId = metadata.Find<int>(0);

                    CuriosityStoreItem item = await Database.Store.ShopDatabase.GetItem(itemId, curiosityUser.Character.CharacterId);
                    item.SkillRequirements = await Database.Store.ShopDatabase.GetSkillRequirements(itemId, curiosityUser.Character.CharacterId);
                    item.ItemRequirements = await Database.Store.ShopDatabase.GetItemRequirements(itemId, curiosityUser.Character.CharacterId);
                    item.RoleRequirements = await Database.Store.ShopDatabase.GetRoleRequirements(itemId, curiosityUser.Character.CharacterId);

                    return item;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "shop:get:items");
                    return null;
                }
            }));

            EventSystem.GetModule().Attach("shop:purchase:item", new AsyncEventCallback(async metadata =>
            {
                SqlResult sqlResult = new SqlResult();
                try
                {
                    CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                    if (curiosityUser.Purchasing)
                    {
                        sqlResult.Message = "Pending Purchase";
                        return sqlResult;
                    }

                    int itemId = metadata.Find<int>(0);
                    int characterId = curiosityUser.Character.CharacterId;

                    curiosityUser.Purchasing = true;

                    CuriosityStoreItem item = new CuriosityStoreItem();

                    goto GetItem;

                GetItem:
                    item = await Database.Store.ShopDatabase.GetItem(itemId, characterId);
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

                        foreach(RoleRequirement roleRequirement in roleRequirements)
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

                        if (metRequirements == requirements) goto AddItemToCharacter;

                        goto FailedPurchaseSkill;
                    }

                    goto AddItemToCharacter; // Let them buy it then!

                AddItemToCharacter:
                    // at this point the item type will be important
                    SqlResult res = await Database.Store.ShopDatabase.PurchaseItem(itemId, characterId);
                    if (res.Success) goto AdjustWallet;
                    goto FailedPurchase;

                AdjustWallet:
                    // if all pass, allow purchase, else fail
                    int minusValue = item.BuyValue * -1;
                    int newWalletValue = await Database.Store.BankDatabase.Adjust(characterId, minusValue);
                    curiosityUser.Character.Cash = newWalletValue;
                    goto SuccessfulPurchase;

                SuccessfulPurchase:
                    curiosityUser.Purchasing = false;
                    sqlResult.Success = true;
                    sqlResult.Message = res.Message;
                    return sqlResult;

                FailedPurchase:
                    curiosityUser.Purchasing = false;
                    sqlResult.Message = res.Message;
                    return sqlResult; // Need to write an error logger

                FailedPurchaseCash:
                    curiosityUser.Purchasing = false;
                    sqlResult.Message = "Not enough cash to buy item";
                    return sqlResult;

                FailedPurchaseRole:
                    curiosityUser.Purchasing = false;
                    sqlResult.Message = "Role Requirement not met";
                    return sqlResult;

                FailedPurchaseItem:
                    curiosityUser.Purchasing = false;
                    sqlResult.Message = "Item Requirement not met";
                    return sqlResult;

                FailedPurchaseSkill:
                    curiosityUser.Purchasing = false;
                    sqlResult.Message = "Skill Requirement not met";
                    return sqlResult;

                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "shop:purchase:item");
                    return sqlResult;
                }
            }));


        }
    }
}
