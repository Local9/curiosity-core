﻿using Curiosity.Core.Server.Extensions;
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
        public static async Task<List<VehicleItem>> GetAllVehicles(int characterId)
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

                if (keyValuePairs.Count == 0)
                    return null;

                item.CharacterVehicleId = keyValuePairs[0]["CharacterVehicleId"];
                item.Label = $"{keyValuePairs[0]["Label"]}";
                item.Hash = $"{keyValuePairs[0]["HashKey"]}";
                item.DatePurchased = keyValuePairs[0]["DatePurchased"];
                item.SpawnTypeId = (SpawnType)keyValuePairs[0]["SpawnTypeId"];

                string vehicleData = $"{keyValuePairs[0]["VehicleData"]}";
                
                if (!string.IsNullOrEmpty(vehicleData))
                {
                    item.VehicleInfo = JsonConvert.DeserializeObject<VehicleInfo>(vehicleData);
                }
            }

            return item;
        }
    }
}