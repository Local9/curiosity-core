using CitizenFX.Core;
using Curiosity.Global.Shared.net.Entity;
using GHMatti.Data.MySQL;
using GHMatti.Data.MySQL.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using Curiosity.Global.Shared.net.Enums;

namespace Curiosity.Server.net.Database
{
    class DatabaseVehicles : BaseScript
    {
        static MySQL mySql;

        public static void Init()
        {
            mySql = Database.mySQL;
        }

        public static async Task<List<VehicleSpawnLocation>> GetVehicleSpawns()
        {
            List<VehicleSpawnLocation> vehicleSpawnLocations = new List<VehicleSpawnLocation>();

            string sql = "CALL curiosity.selVehicleSpawnPoints();";

            using (var result = mySql.QueryResult(sql))
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
                        spawnBlipName = $"{k["spawnTypeId"]}"
                    };

                    vehicleSpawnLocation.X = float.Parse($"{k["x"]}");
                    vehicleSpawnLocation.Y = float.Parse($"{k["y"]}");
                    vehicleSpawnLocation.Z = float.Parse($"{k["z"]}");

                    vehicleSpawnLocations.Add(vehicleSpawnLocation);
                }
            }
            return vehicleSpawnLocations;
        }

        public static async Task<List<Inventory>> GetVehicleSpawnsForLocation(SpawnLocations spawnLocation)
        {
            List<Inventory> inventoryList = new List<Inventory>();

            string sql = "CALL curiosity.selVehiclesSpawnsForLocation(@spawnLocation);";
            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@spawnLocation", (int)spawnLocation);

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

        public static async Task<List<Inventory>> GetVehicleList(VehicleSpawnTypes vehicleSpawnType, SpawnLocations spawnLocation)
        {
            List<Inventory> inventoryList = new List<Inventory>();

            string sql = "CALL curiosity.selVehiclesForSpawnLocation(@spawnType, @spawnLocation);";
            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@spawnType", (int)vehicleSpawnType);
            myParams.Add("@spawnLocation", (int)spawnLocation);

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
