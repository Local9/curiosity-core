using CitizenFX.Core;
using Curiosity.Global.Shared.net.Entity;
using GHMatti.Data.MySQL;
using GHMatti.Data.MySQL.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Server.net.Database
{
    class DatabaseCharacterInventory : BaseScript
    {
        static MySQL mySql;

        public static void Init()
        {
            mySql = Database.mySQL;
        }

        public static async Task<List<Inventory>> GetInventoryAsync(int characterId)
        {
            List<Inventory> inventoryList = new List<Inventory>();

            string sql = "CALL curiosity.spGetCharacterInventory(@characterId);";
            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@characterId", characterId);

            using (var result = mySql.QueryResult(sql, myParams))
            {
                ResultSet keyValuePairs = await result;
                await Delay(0);
                if (keyValuePairs.Count == 0)
                {
                    return inventoryList;
                }

                foreach (Dictionary<string, object> k in keyValuePairs)
                {
                    Inventory inventory = new Inventory
                    {
                        ItemId = int.Parse($"{k["itemId"]}"),
                        NumberOfItems = int.Parse($"{k["numberOfItem"]}"),
                        Limit = int.Parse($"{k["limit"]}"),
                        Description = $"{k["description"]}",
                        Icon = $"{k["icon"]}"
                    };
                    inventoryList.Add(inventory);
                }
            }
            return inventoryList;
        }
    }
}
