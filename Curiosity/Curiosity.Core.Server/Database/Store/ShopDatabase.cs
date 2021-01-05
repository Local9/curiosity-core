using Curiosity.Core.Server.Diagnostics;
using Curiosity.Systems.Library.Models;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Database.Store
{
    class ShopDatabase
    {
        public static async Task<List<Tuple<int, string>>> GetCategories(int storeOnly = 1)
        {
            List<Tuple<int, string>> lstCategories = new List<Tuple<int, string>>();

            try
            {

                Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@StoreOnly", storeOnly },
                };

                string myQuery = "CALL selItemCategories(@StoreOnly);";

                using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
                {
                    ResultSet keyValuePairs = await result;

                    if (keyValuePairs.Count == 0)
                    {
                        lstCategories.Add(new Tuple<int, string>(0, "No Categories"));
                    }
                    else
                    {
                        foreach (Dictionary<string, object> kv in keyValuePairs)
                        {
                            Tuple<int, string> tuple = new Tuple<int, string>(
                                int.Parse($"{kv["ItemCategoryId"]}"),
                                $"{kv["Label"]}"
                                );
                            lstCategories.Add(tuple);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
                lstCategories.Add(new Tuple<int, string>(0, "No Categories"));
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

                string myQuery = "CALL selStoreItemsByCategoryId(@itemCategoryId, @characterId);";

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
                            curiosityStoreItem.ItemId = int.Parse($"{kv["ItemId"]}");
                            curiosityStoreItem.Label = $"{kv["Label"]}";
                            curiosityStoreItem.Description = $"{kv["Description"]}";
                            curiosityStoreItem.BuyValue = int.Parse($"{kv["BuyValue"]}");
                            curiosityStoreItem.BuyBackValue = int.Parse($"{kv["BuyBackValue"]}");
                            curiosityStoreItem.NumberInStock = int.Parse($"{kv["NumberInStock"]}");
                            curiosityStoreItem.ItemPurchased = int.Parse($"{kv["ItemPurchased"]}") == 1;
                            curiosityStoreItem.NumberOwned = int.Parse($"{kv["NumberOwned"]}");
                            curiosityStoreItem.IsStockControlled = int.Parse($"{kv["IsStockControlled"]}") == 1;
                            curiosityStoreItem.MaximumAllowed = int.Parse($"{kv["MaximumAllowed"]}");

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

        public static async Task<Tuple<bool, int>> TradeItem(int characterId, int itemId, int numberOfItems, bool purchase)
        {
            Tuple<bool, int> rtValue = new Tuple<bool, int>(false, 0);

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
                        return new Tuple<bool, int>($"{kv[0]["Result"]}" == "1", int.Parse($"{kv[0]["ItemValue"]}"));
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
