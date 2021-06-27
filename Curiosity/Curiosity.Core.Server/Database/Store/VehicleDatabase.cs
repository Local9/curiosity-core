using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Models;
using GHMatti.Data.MySQL.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Database.Store
{
    class VehicleDatabase
    {
        public static async Task<List<VehicleItem>> Get(int characterId)
        {
            List<VehicleItem> lst = new List<VehicleItem>();
            
            Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@characterId", characterId }
                };

            string myQuery = "call selCharacterVehicles(@characterId);";

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                if (keyValuePairs.Count == 0)
                    return null;

                foreach (Dictionary<string, object> kv in keyValuePairs)
                {
                    VehicleItem item = new VehicleItem();

                    item.CharacterVehicleId = kv["CharacterVehicleId"].ToInt();
                    item.Label = $"{kv["Label"]}";
                    item.Hash = $"{kv["HashKey"]}";
                    item.DatePurchased = kv["DatePurchased"].ToDateTime();

                    item.VehicleInfo = JsonConvert.DeserializeObject<VehicleInfo>($"{kv["VehicleData"]}");

                    if (string.IsNullOrEmpty(item.VehicleInfo.plateText)) item.VehicleInfo.plateText = $"#{characterId}";

                    lst.Add(item);
                }
            }

            return lst;
        }
    }
}
