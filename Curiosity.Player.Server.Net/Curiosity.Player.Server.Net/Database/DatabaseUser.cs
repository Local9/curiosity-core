using GHMatti.Data.MySQL;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CitizenFX.Core;

namespace Curiosity.Server.Net.Database
{
    public class DatabaseUser : BaseScript
    {
        MySQL mySql;

        static DatabaseUser databaseUser;

        public static DatabaseUser GetInstance()
        {
            return databaseUser;
        }

        public DatabaseUser()
        {
            mySql = DatabaseSettings.GetInstance().mySQL;
            databaseUser = this;
        }

        public async Task<Entity.User> GetUserIdAsync(string steamId)
        {
            try
            {
                Entity.User user = new Entity.User();

                Dictionary<string, object> myParams = new Dictionary<string, object>();
                myParams.Add("@steamId", steamId);
                string selectQuery = "select userId, locationId from users where steamId = @steamId";
                using (var result = mySql.QueryResult(selectQuery, myParams))
                {
                    ResultSet keyValuePairs = await result;
                    await Delay(0);
                    if (keyValuePairs.Count == 0)
                    {
                        string myInsertQuery = "insert into users (steamId) values (@steamId);";
                        user.UserId = await mySql.Query(myInsertQuery, myParams, true);
                        user.LocationId = 1;
                        await Delay(0);
                    }

                    foreach (Dictionary<string, object> keyValues in keyValuePairs)
                    {
                        user.UserId = long.Parse($"{keyValues["userId"]}");
                        user.LocationId = long.Parse($"{keyValues["locationId"]}");
                    }
                    return user;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetUserIdAsync -> {ex.Message}");
                return null;
            }
        }

        public async Task<Vector3> GetUserPositionAsync(long locationId)
        {
            try
            {
                Vector3 vector3 = new Vector3();

                Dictionary<string, object> myParams = new Dictionary<string, object>();
                myParams.Add("@locationId", locationId);
                string selectQuery = "select x, y, z from locations where locationId = @locationId";
                using (var result = mySql.QueryResult(selectQuery, myParams))
                {
                    ResultSet keyValuePairs = await result;
                    await Delay(0);

                    foreach (Dictionary<string, object> keyValues in keyValuePairs)
                    {
                        vector3.X = float.Parse($"{keyValues["x"]}");
                        vector3.Y = float.Parse($"{keyValues["y"]}");
                        vector3.Z = float.Parse($"{keyValues["z"]}");
                    }
                    return vector3;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetUserIdAsync -> {ex.Message}");
                return new Vector3();
            }
        }

        public async Task TestQueryAsync()
        {
            await mySql.Query("SELECT 1;");
        }
    }
}
