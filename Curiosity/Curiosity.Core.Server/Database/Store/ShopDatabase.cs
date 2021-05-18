using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.Shop;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Database.Store
{
    class ShopDatabase
    {
        public static async Task<List<ShopCategory>> GetCategories()
        {
            List<ShopCategory> lstCategories = new List<ShopCategory>();

            try
            {
                string myQuery = "CALL selStoreCategories();";

                using (var result = MySqlDatabase.mySQL.QueryResult(myQuery))
                {
                    ResultSet keyValuePairs = await result;

                    if (keyValuePairs.Count == 0)
                    {
                        lstCategories.Add(new ShopCategory() { ShopCategoryID = 0, ShopCategoryDescription = "No Categories" });
                    }
                    else
                    {
                        foreach (Dictionary<string, object> kv in keyValuePairs)
                        {
                            ShopCategory shopCategory = new ShopCategory();
                            shopCategory.ShopCategoryID = kv["ShopCategoryId"].ToInt();
                            shopCategory.ShopCategoryDescription = $"{kv["Description"]}";

                            var shopCat = lstCategories.Select(x => x).Where(x => x.ShopCategoryID == kv["ShopCategoryId"].ToInt()).FirstOrDefault();

                            if (shopCat is null)
                                lstCategories.Add(shopCategory);
                        }

                        foreach (Dictionary<string, object> kv in keyValuePairs)
                        {
                            ShopCategoryItem categoryItem = new ShopCategoryItem();
                            categoryItem.ShopCategoryItemID = kv["ItemCategoryId"].ToInt();
                            categoryItem.ShopCategoryItemDescription = $"{kv["Label"]}";

                            ShopCategory sc = lstCategories.Select(x => x).Where(x => x.ShopCategoryID == kv["ShopCategoryId"].ToInt()).FirstOrDefault();
                            if (sc is not null)
                                sc.Categories.Add(categoryItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
                lstCategories.Add(new ShopCategory() { ShopCategoryID = 0, ShopCategoryDescription = "No Categories" });
            }

            return lstCategories;
        }

        internal async static Task<List<RoleRequirement>> GetRoleRequirements(int itemId, int characterId)
        {
            List<RoleRequirement> lst = new List<RoleRequirement>();
            try
            {
                Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@itemId", itemId },
                    { "@characterId", characterId },
                };

                string myQuery = "CALL selItemRequiredRoles(@itemId, @characterId);";

                using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
                {
                    ResultSet keyValuePairs = await result;

                    if (keyValuePairs.Count == 0)
                    {
                        return lst;
                    }

                    foreach (Dictionary<string, object> kv in keyValuePairs)
                    {
                        RoleRequirement role = new RoleRequirement();
                        role.RoleId = int.Parse($"{kv["roleId"]}");
                        role.Description = $"{kv["Description"]}";
                        role.HasRole = int.Parse($"{kv["HasRole"]}") == 1;

                        lst.Add(role);
                    }

                    return lst;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
                return lst;
            }
        }

        internal async static Task<List<ItemRequirement>> GetItemRequirements(int itemId, int characterId)
        {
            List<ItemRequirement> lst = new List<ItemRequirement>();
            try
            {
                Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@itemId", itemId },
                    { "@characterId", characterId },
                };

                string myQuery = "CALL selItemRequiredItems(@itemId, @characterId);";

                using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
                {
                    ResultSet keyValuePairs = await result;

                    if (keyValuePairs.Count == 0)
                    {
                        return lst;
                    }

                    foreach (Dictionary<string, object> kv in keyValuePairs)
                    {
                        ItemRequirement item = new ItemRequirement();
                        item.ItemLabel = $"{kv["Label"]}";
                        item.AmountRequired = int.Parse($"{kv["RequiredValue"]}");
                        item.AmountCurrent = int.Parse($"{kv["AmountOwned"]}");
                        item.RequirementMet = int.Parse($"{kv["RequirementMet"]}") == 1;

                        lst.Add(item);
                    }

                    return lst;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
                return lst;
            }
        }

        internal async static Task<List<SkillRequirement>> GetSkillRequirements(int itemId, int characterId)
        {
            List<SkillRequirement> lst = new List<SkillRequirement>();
            try
            {
                Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@itemId", itemId },
                    { "@characterId", characterId },
                };

                string myQuery = "CALL selItemRequiredSkills(@itemId, @characterId);";

                using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
                {
                    ResultSet keyValuePairs = await result;

                    if (keyValuePairs.Count == 0)
                    {
                        return lst;
                    }

                    foreach (Dictionary<string, object> kv in keyValuePairs)
                    {
                        SkillRequirement skill = new SkillRequirement();
                        skill.SkillLabel = $"{kv["Label"]}";
                        skill.ExperienceRequired = int.Parse($"{kv["RequiredValue"]}");
                        skill.ExperienceCurrent = int.Parse($"{kv["Experience"]}");
                        skill.RequirementMet = int.Parse($"{kv["RequirementMet"]}") == 1;
                        
                        lst.Add(skill);
                    }

                    return lst;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
                return lst;
            }
        }

        internal static async Task<SqlResult> PurchaseItem(int itemId, int characterId)
        {
            SqlResult rtValue = new SqlResult();
            try
            {
                Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@itemId", itemId },
                    { "@characterId", characterId },
                };

                string myQuery = "CALL spShopPurchaseItem(@characterId, @itemId);";

                using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
                {
                    ResultSet kv = await result;

                    if (kv.Count == 0)
                    {
                        return rtValue;
                    }

                    rtValue.Success = $"{kv[0]["Result"]}" == "1";
                    rtValue.ItemValue = int.Parse($"{kv[0]["CurrentlyOwned"]}");
                    rtValue.Message = $"{kv[0]["Message"]}";

                    return rtValue;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
                return rtValue;
            }
        }

        internal static async Task<CuriosityStoreItem> GetItem(int itemId, int characterId)
        {
            try
            {
                Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@itemId", itemId },
                    { "@characterId", characterId },
                };

                string myQuery = "CALL selShopItem(@itemId, @characterId);";

                using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
                {
                    ResultSet kv = await result;

                    if (kv.Count == 0)
                    {
                        return null;
                    }

                    CuriosityStoreItem curiosityStoreItem = new CuriosityStoreItem();
                    curiosityStoreItem.ShopItemId = int.Parse($"{kv[0]["ShopItemId"]}");
                    curiosityStoreItem.ItemId = int.Parse($"{kv[0]["ItemId"]}");
                    curiosityStoreItem.Label = $"{kv[0]["Label"]}";
                    curiosityStoreItem.Description = $"{kv[0]["Description"]}";
                    curiosityStoreItem.BuyValue = int.Parse($"{kv[0]["BuyValue"]}");
                    curiosityStoreItem.BuyBackValue = int.Parse($"{kv[0]["BuyBackValue"]}");
                    curiosityStoreItem.NumberInStock = int.Parse($"{kv[0]["NumberInStock"]}");
                    curiosityStoreItem.ItemPurchased = int.Parse($"{kv[0]["ItemPurchased"]}") == 1;
                    curiosityStoreItem.NumberOwned = int.Parse($"{kv[0]["NumberOwned"]}");
                    curiosityStoreItem.IsStockManaged = int.Parse($"{kv[0]["IsStockManaged"]}") == 1;
                    curiosityStoreItem.MaximumAllowed = int.Parse($"{kv[0]["MaximumAllowed"]}");
                    curiosityStoreItem.HasRoleRequirement = int.Parse($"{kv[0]["HasRoleRequirement"]}") == 1;
                    curiosityStoreItem.NumberOfSkillRequirements = int.Parse($"{kv[0]["NumberSkillRequirements"]}");
                    curiosityStoreItem.NumberOfItemRequirements = int.Parse($"{kv[0]["NumberItemRequirements"]}");
                    curiosityStoreItem.ImageUri = $"{kv[0]["ImageUri"]}";

                    return curiosityStoreItem;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
                return null;
            }
        }

        public static async Task<List<CuriosityStoreItem>> GetCategoryItems(int categoryId, int characterId)
        {
            try
            {
                Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@itemCategoryId", categoryId },
                    { "@characterId", characterId },
                };

                string myQuery = "CALL selShopItemsByCategoryId(@itemCategoryId, @characterId);";

                using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
                {
                    ResultSet keyValuePairs = await result;

                    if (keyValuePairs.Count == 0)
                    {
                        return null;
                    }
                    else
                    {
                        List<CuriosityStoreItem> lstItems = new List<CuriosityStoreItem>();

                        foreach (Dictionary<string, object> kv in keyValuePairs)
                        {
                            CuriosityStoreItem curiosityStoreItem = new CuriosityStoreItem();
                            curiosityStoreItem.ShopItemId = int.Parse($"{kv["ShopItemId"]}");
                            curiosityStoreItem.ItemId = int.Parse($"{kv["ItemId"]}");
                            curiosityStoreItem.Label = $"{kv["Label"]}";
                            curiosityStoreItem.Description = $"{kv["Description"]}";
                            curiosityStoreItem.BuyValue = int.Parse($"{kv["BuyValue"]}");
                            curiosityStoreItem.BuyBackValue = int.Parse($"{kv["BuyBackValue"]}");
                            curiosityStoreItem.NumberInStock = int.Parse($"{kv["NumberInStock"]}");
                            curiosityStoreItem.ItemPurchased = int.Parse($"{kv["ItemPurchased"]}") == 1;
                            curiosityStoreItem.NumberOwned = int.Parse($"{kv["NumberOwned"]}");
                            curiosityStoreItem.IsStockManaged = int.Parse($"{kv["IsStockManaged"]}") == 1;
                            curiosityStoreItem.MaximumAllowed = int.Parse($"{kv["MaximumAllowed"]}");
                            curiosityStoreItem.HasRoleRequirement = int.Parse($"{kv["HasRoleRequirement"]}") == 1;
                            curiosityStoreItem.NumberOfSkillRequirements = int.Parse($"{kv["NumberSkillRequirements"]}");
                            curiosityStoreItem.NumberOfItemRequirements = int.Parse($"{kv["NumberItemRequirements"]}");
                            curiosityStoreItem.ImageUri = $"{kv["ImageUri"]}";

                            lstItems.Add(curiosityStoreItem);
                        }

                        return lstItems;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
                return null;
            }
        }

        internal static async Task<bool> Adjust(int shopItemId, int amount)
        {
            Dictionary<string, object> myParams = new Dictionary<string, object>()
            {
                { "@ShopItemId", shopItemId },
                { "@Amount", amount }
            };

            string myQuery = "CALL spShopAdjustStock(@ShopItemId, @Amount);";

            return await MySqlDatabase.mySQL.Query(myQuery, myParams) > 0;
        }

        //public static async Task<SqlResult> TradeItem(int characterId, int itemId, int numberOfItems, bool purchase)
        //{
        //    SqlResult rtValue = new SqlResult();

        //    try
        //    {
        //        Dictionary<string, object> myParams = new Dictionary<string, object>()
        //        {
        //            { "@characterId", characterId },
        //            { "@itemId", itemId },
        //            { "@numberOfItems", numberOfItems },
        //            { "@purchase", purchase },
        //        };

        //        string myQuery = "CALL spCharacterItem(@characterId, @itemId, @numberOfItems, @purchase);";

        //        using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
        //        {
        //            ResultSet kv = await result;

        //            if (kv.Count == 0)
        //            {
        //                return rtValue;
        //            }
        //            else
        //            {
        //                rtValue.Success = $"{kv[0]["Result"]}" == "1";
        //                rtValue.ItemValue = int.Parse($"{kv[0]["ItemValue"]}");
        //                rtValue.Message = $"{kv[0]["Message"]}";
        //                return rtValue;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error($"{ex}");
        //        return rtValue;
        //    }
        //}
    }
}
