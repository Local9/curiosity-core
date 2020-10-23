using CitizenFX.Core;
using Curiosity.Global.Shared.Data;
using Curiosity.Global.Shared.Entity;
using Curiosity.Global.Shared.Utils;
using Curiosity.Shared.Server.net.Helpers;
using GHMatti.Data.MySQL;
using GHMatti.Data.MySQL.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace Curiosity.Server.net.Database
{
    class DatabaseVehicles : BaseScript
    {
        static MySQL mySql;

        public static void Init()
        {
            mySql = Database.mySQL;
        }

        public static async Task<List<VehicleSpawnLocation>> GetVehicleSpawns(int serverId)
        {
            List<VehicleSpawnLocation> vehicleSpawnLocations = new List<VehicleSpawnLocation>();

            string sql = "CALL curiosity.selVehicleSpawnPoints(@serverId);";
            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@serverId", serverId);

            using (var result = mySql.QueryResult(sql, myParams))
            {
                ResultSet keyValuePairs = await result;
                await Delay(0);
                if (keyValuePairs.Count == 0)
                {
                    return vehicleSpawnLocations;
                }

                foreach (Dictionary<string, object> k in keyValuePairs)
                {
                    VehicleSpawnLocation vehicleSpawnLocation = new VehicleSpawnLocation
                    {
                        spawnId = int.Parse($"{k["spawnId"]}"),
                        spawnTypeId = int.Parse($"{k["spawnTypeId"]}"),
                        spawnDescription = $"{k["description"]}",
                        spawnMarker = int.Parse($"{k["spawnMarkerId"]}"),
                        spawnBlip = int.Parse($"{k["blipTypeId"]}"),
                        spawnBlipColor = int.Parse($"{k["blipColorId"]}"),
                        spawnBlipName = $"{k["blipName"]}"
                    };

                    vehicleSpawnLocation.X = float.Parse($"{k["x"]}");
                    vehicleSpawnLocation.Y = float.Parse($"{k["y"]}");
                    vehicleSpawnLocation.Z = float.Parse($"{k["z"]}");

                    vehicleSpawnLocations.Add(vehicleSpawnLocation);
                }
            }
            return vehicleSpawnLocations;
        }

        public static async Task<List<VehicleItem>> GetVehiclesForSpawn(int spawnId)
        {
            List<VehicleItem> vehicleItems = new List<VehicleItem>();

            string sql = "CALL curiosity.selVehicleSpawnList(@spawnIdIn);";
            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@spawnIdIn", spawnId);

            using (var result = mySql.QueryResult(sql, myParams))
            {
                ResultSet keyValuePairs = await result;
                await Delay(0);
                if (keyValuePairs.Count == 0)
                {
                    return vehicleItems;
                }

                foreach (Dictionary<string, object> k in keyValuePairs)
                {
                    VehicleItem vehicleItem = new VehicleItem
                    {
                        Name = $"{k["vehicleLabel"]}",
                        VehicleHashString = $"{k["vehicleHash"]}",
                        UnlockRequiredSkill = $"{k["requiredSkill"]}",
                        UnlockRequiredSkillDescription = $"{k["requiredSkillDescription"]}",
                        UnlockRequirementValue = long.Parse($"{k["vehicleUnlockRequirement"]}"),
                        SpawnPositionX = float.Parse($"{k["x"]}"),
                        SpawnPositionY = float.Parse($"{k["y"]}"),
                        SpawnPositionZ = float.Parse($"{k["z"]}"),
                        SpawnHeading = float.Parse($"{k["spawnOutputHeading"]}"),
                        InstallSirens = $"{k["installSirens"]}" == "1"
                    };
                    vehicleItems.Add(vehicleItem);
                }
            }
            return vehicleItems;
        }

        public static async Task<List<VehicleItem>> GetVehiclesForDonators(long userID)
        {
            List<VehicleItem> vehicleItems = new List<VehicleItem>();

            string sql = "CALL curiosity.selVehiclesForDonators(@serverId, @userId);";
            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@serverId", Server.serverId);
            myParams.Add("@userId", userID);

            using (var result = mySql.QueryResult(sql, myParams))
            {
                ResultSet keyValuePairs = await result;
                await Delay(0);
                if (keyValuePairs.Count == 0)
                {
                    return vehicleItems;
                }

                foreach (Dictionary<string, object> k in keyValuePairs)
                {
                    VehicleItem vehicleItem = new VehicleItem
                    {
                        Name = $"{k["vehicleLabel"]}",
                        VehicleHashString = $"{k["vehicleHash"]}",
                        InstallSirens = $"{k["installSirens"]}" == "1"
                    };
                    vehicleItems.Add(vehicleItem);
                }
            }
            return vehicleItems;
        }

        public static async Task<List<VehicleShopItem>> GetVehicleShopItems(int serverId, long characterId)
        {
            List<VehicleShopItem> vehicleShopItems = new List<VehicleShopItem>();

            string sql = "CALL curiosity.selVehiclesFromShop(@serverId, @charId);";
            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@serverId", serverId);
            myParams.Add("@charId", characterId);

            using (var result = mySql.QueryResult(sql, myParams))
            {
                ResultSet keyValuePairs = await result;
                await Delay(0);
                if (keyValuePairs.Count == 0)
                {
                    return vehicleShopItems;
                }

                foreach (Dictionary<string, object> k in keyValuePairs)
                {
                    VehicleShopItem vehicleShopItem = new VehicleShopItem
                    {
                        Id = $"{k["vehicleShopId"]}".ToInt(),
                        Label = $"{k["label"]}",
                        Cost = $"{k["cost"]}".ToInt(),
                    };

                    string limitedNumber = $"{k["limitedNumber"]}";
                    string image = $"{k["image"]}";
                    string charId = $"{k["characterId"]}";

                    if (!string.IsNullOrEmpty(limitedNumber))
                        vehicleShopItem.NumberRemaining = limitedNumber.ToInt();

                    if (!string.IsNullOrEmpty(image))
                        vehicleShopItem.ImageUri = image;

                    vehicleShopItem.IsOwned = !string.IsNullOrEmpty(charId);

                    vehicleShopItems.Add(vehicleShopItem);
                }
            }
            return vehicleShopItems;
        }

        public static async Task<bool> InsertCharacterVehicle(long characterId, int vehShopId)
        {
            string sql = "CALL curiosity.insCharacterVehicle(@charId, @vehShopId);";
            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@charId", characterId);
            myParams.Add("@vehShopId", vehShopId);

            using (var result = mySql.QueryResult(sql, myParams))
            {
                ResultSet keyValuePairs = await result;
                await Delay(10);
                if (keyValuePairs.Count == 0)
                {
                    return false;
                }

                bool success = $"{keyValuePairs[0]["success"]}" == "1";

                // Log.Info($"{keyValuePairs[0]["success"]}/{success}");

                return success;
            }
        }

        public static async Task DeleteCharacterVehicle(long characterId, int vehShopId)
        {
            string sql = "CALL curiosity.delCharacterVehicle(@charId, @vehShopId);";
            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@charId", characterId);
            myParams.Add("@vehShopId", vehShopId);
            mySql.Query(sql, myParams);
        }

        public static async Task<bool> SelectCharacterVehicle(long characterId, int vehShopId)
        {
            string sql = "CALL curiosity.selCharacterVehicle(@charId, @vehShopId);";
            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@charId", characterId);
            myParams.Add("@vehShopId", vehShopId);

            using (var result = mySql.QueryResult(sql, myParams))
            {
                ResultSet keyValuePairs = await result;
                await Delay(0);
                if (keyValuePairs.Count == 0)
                {
                    // doesn't own it
                    return false;
                }

                // owns it
                return true;
            }
        }

        public static async Task<VehicleShopItem> SelectVehicleShopItem(int vehShopId)
        {
            string sql = "CALL curiosity.selVehicleFromShop(@vehShopId);";
            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@vehShopId", vehShopId);

            using (var result = mySql.QueryResult(sql, myParams))
            {
                ResultSet keyValuePairs = await result;
                await Delay(0);
                if (keyValuePairs.Count == 0)
                {
                    return null;
                }

                Dictionary<string, object> k = keyValuePairs[0];

                VehicleShopItem vehicleShopItem = new VehicleShopItem
                {
                    Id = $"{k["vehicleShopId"]}".ToInt(),
                    Label = $"{k["label"]}",
                    Cost = $"{k["cost"]}".ToInt(),
                    VehicleHash = $"{k["vehicleHash"]}",
                };

                string limitedNumber = $"{k["limitedNumber"]}";
                string image = $"{k["image"]}";

                if (!string.IsNullOrEmpty(limitedNumber))
                    vehicleShopItem.NumberRemaining = limitedNumber.ToInt();

                if (!string.IsNullOrEmpty(image))
                    vehicleShopItem.ImageUri = image;

                return vehicleShopItem;
            }
        }

        
    }
}
