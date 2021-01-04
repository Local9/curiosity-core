using CitizenFX.Core;
using Curiosity.Global.Shared.Enums;
using Curiosity.Shared.Server.net.Helpers;
using GHMatti.Data.MySQL;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GlobalEntity = Curiosity.Global.Shared.Entity;

namespace Curiosity.Server.net.Database
{
    class DatabaseShop : BaseScript
    {
        static MySQL mySQL;

        public static void Init()
        {
            mySQL = Database.mySQL;
        }

        public static async Task<List<Tuple<int, string>>> GetCategories(int StoreOnly = 1)
        {
            if (StoreOnly > 1)
            {
                throw new Exception($"StoreOnly can only use the value 0 or 1, mysql uses tinyint(4) for booleans");
            }

            List<Tuple<int, string>> lstCategories = new List<Tuple<int, string>>();

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@StoreOnly", StoreOnly);

            string selectQuery = "CALL curiosity.selItemCategories(@StoreOnly);";

            using (var result = mySQL.QueryResult(selectQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                await Delay(200);

                if (keyValuePairs.Count == 0)
                {
                    lstCategories.Add(new Tuple<int, string>(0, "No Categories"));
                }
                else
                {
                    foreach (Dictionary<string, object> keyValues in keyValuePairs)
                    {
                        Tuple<int, string> tuple = new Tuple<int, string>(
                            int.Parse($"{keyValues["ItemCategoryId"]}"),
                            $"{keyValues["Label"]}"
                            );
                        lstCategories.Add(tuple);
                    }
                }

                return lstCategories;
            }
        }
    }
}
