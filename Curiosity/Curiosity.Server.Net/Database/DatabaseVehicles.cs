using CitizenFX.Core;
using Curiosity.Global.Shared.net.Entity;
using GHMatti.Data.MySQL;
using GHMatti.Data.MySQL.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public static async Task<List<VehicleItem>> GetVehiclesForDonators(int userID)
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
    }
}
