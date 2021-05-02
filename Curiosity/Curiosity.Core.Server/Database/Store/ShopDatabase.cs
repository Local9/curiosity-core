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

        public static async Task<SqlResult> TradeItem(int characterId, int itemId, int numberOfItems, bool purchase)
        {
            SqlResult rtValue = new SqlResult();

            try
            {
                Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@characterId", characterId },
                    { "@itemId", itemId },
                    { "@numberOfItems", numberOfItems },
                    { "@purchase", purchase },
                };

                string myQuery = "CALL spCharacterItem(@characterId, @itemId, @numberOfItems, @purchase);";

                using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
                {
                    ResultSet kv = await result;

                    if (kv.Count == 0)
                    {
                        return rtValue;
                    }
                    else
                    {
                        rtValue.Success = $"{kv[0]["Result"]}" == "1";
                        rtValue.ItemValue = int.Parse($"{kv[0]["ItemValue"]}");
                        rtValue.Message = $"{kv[0]["Message"]}";
                        return rtValue;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
                return rtValue;
            }
        }
    }
}
