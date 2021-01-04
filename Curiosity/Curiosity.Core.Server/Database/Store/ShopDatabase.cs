using Curiosity.Core.Server.Diagnostics;
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
    }
}
