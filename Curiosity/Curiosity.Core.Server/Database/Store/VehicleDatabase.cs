using CitizenFX.Core;
using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Enums;
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
        public static async Task<List<VehicleItem>> GetAllVehicles(int characterId, bool carsOnly = false)
        {
            List<VehicleItem> lst = new List<VehicleItem>();

            Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@characterId", characterId },
                    { "@pCarsOnly", carsOnly }
                };

            string myQuery = "call selCharacterVehicles(@characterId, @pCarsOnly);";

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                await BaseScript.Delay(0);

                if (keyValuePairs.Count == 0)
                    return lst;

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
        public static async Task<VehicleItem> GetVehicle(int characterVehicleId)
        {
            VehicleItem item = new();

            Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@characterVehicleId", characterVehicleId }
                };

            string myQuery = "call selCharacterVehicle(@characterVehicleId);";

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                await BaseScript.Delay(0);

                if (keyValuePairs.Count == 0)
                    return null;

                item.CharacterVehicleId = keyValuePairs[0]["CharacterVehicleId"];
                item.Label = $"{keyValuePairs[0]["Label"]}";
                item.Hash = $"{keyValuePairs[0]["HashKey"]}";
                item.DatePurchased = keyValuePairs[0]["DatePurchased"];
                item.SpawnTypeId = (SpawnType)keyValuePairs[0]["SpawnTypeId"];
                item.BuyBackValue = keyValuePairs[0]["BuyBackValue"];
                item.TicketsOutstanding = $"{keyValuePairs[0]["HasTickets"]}" == "true";
                item.TicketsOverdue = $"{keyValuePairs[0]["HasOverdueTickets"]}" == "true";

                string vehicleData = $"{keyValuePairs[0]["VehicleData"]}";

                if (!string.IsNullOrEmpty(vehicleData))
                    item.VehicleInfo = JsonConvert.DeserializeObject<VehicleInfo>(vehicleData);

                if (DateTime.TryParse($"{keyValuePairs[0]["DateDeleted"]}", out DateTime dateDeleted))
                    item.DateDeleted = dateDeleted;
            }

            return item;
        }

        internal static async Task<bool> SaveVehicle(int characterId, int vehId, string json)
        {
            Dictionary<string, object> myParams = new Dictionary<string, object>()
            {
                { "@CharacterID", characterId },
                { "@VehicleId", vehId },
                { "@JSON", json },
            };

            string myQuery = "CALL upVehicle(@CharacterID, @VehicleId, @JSON);";

            return await MySqlDatabase.mySQL.Query(myQuery, myParams) > 0;
        }

        internal static async Task<bool> MarkVehicleDeleted(int characterVehicleId)
        {
            Dictionary<string, object> myParams = new Dictionary<string, object>()
            {
                { "@inCharacterVehicleId", characterVehicleId },
            };

            string myQuery = "CALL delCharacterVehicle(@inCharacterVehicleId);";

            return await MySqlDatabase.mySQL.Query(myQuery, myParams) > 0;
        }
    }
}
