using GHMatti.Data.MySQL;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CitizenFX.Core;

namespace Curiosity.Server.net.Database
{
    public class DatabaseUsers : BaseScript
    {
        MySQL mySql;

        static DatabaseUsers databaseUser;

        public static DatabaseUsers GetInstance()
        {
            return databaseUser;
        }

        public DatabaseUsers()
        {
            mySql = DatabaseSettings.GetInstance().mySQL;
            databaseUser = this;
        }

        public async Task<Entity.User> GetUserAsync(string steamId)
        {
            try
            {
                Entity.User user = new Entity.User();

                Dictionary<string, object> myParams = new Dictionary<string, object>();
                myParams.Add("@steamId", steamId);

                string selectQuery = "select" +
                    " users.userId, users.locationId, roles.roleId, roles.description, IFNULL(userskills.experience, 0) as experience" +
                    " from" +
                    " users" +
                    " inner join roles on users.roleId = roles.roleId" +
                    " left join userskills on users.userId = userskills.userId and userskills.skillId = 1" +
                    " where steamId = @steamId";

                using (var result = mySql.QueryResult(selectQuery, myParams))
                {
                    ResultSet keyValuePairs = await result;
                    await Delay(0);
                    if (keyValuePairs.Count == 0)
                    {
                        string myInsertQuery = "insert into users (steamId) values (@steamId);";
                        user.UserId = await mySql.Query(myInsertQuery, myParams, true);
                        user.LocationId = 1;
                        user.RoleId = 1;
                        await Delay(0);
                    }

                    foreach (Dictionary<string, object> keyValues in keyValuePairs)
                    {
                        user.UserId = long.Parse($"{keyValues["userId"]}");
                        user.LocationId = long.Parse($"{keyValues["locationId"]}");
                        user.RoleId = int.Parse($"{keyValues["roleId"]}");
                        user.Role = $"{keyValues["description"]}";
                        user.WorldExperience = long.Parse($"{keyValues["experience"]}");
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

        public async Task UpdatePlayerLocation(long locationId, float x, float y, float z)
        {
            try
            {
                Dictionary<string, object> myParams = new Dictionary<string, object>();
                myParams.Add("@locationId", locationId);
                myParams.Add("@x", x);
                myParams.Add("@y", y);
                myParams.Add("@z", z);

                string selectQuery = "update locations set x = @x, y = @y, z = @z where locationId = @locationId";
                await mySql.Query(selectQuery, myParams, false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdatePlayerLocation -> {ex.Message}");
            }
        }

        public async Task UpdateUserLocationId(long userId, long locationId)
        {
            try
            {
                Dictionary<string, object> myParams = new Dictionary<string, object>();
                myParams.Add("@userId", userId);
                myParams.Add("@locationId", locationId);

                string selectQuery = "update users set locationId = @locationId where userId = @userId";
                await mySql.Query(selectQuery, myParams, false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateUserLocationId -> {ex.Message}");
            }
        }

        public async Task<long> SaveLocationAsync(int locationType, float x, float y, float z)
        {
            try
            {
                Dictionary<string, object> myParams = new Dictionary<string, object>();
                myParams.Add("@locationType", locationType);
                myParams.Add("@x", x);
                myParams.Add("@y", y);
                myParams.Add("@z", z);

                string selectQuery = "insert into locations (locationType, locationDescription, x, y, z) values (@locationType, 'Player', @x, @y, @z)";
                return await mySql.Query(selectQuery, myParams, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveLocationAsync -> {ex.Message}");
                return 0;
            }
        }

        public async Task<Vector3> GetUserLocationAsync(long locationId)
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
                Debug.WriteLine($"GetUserLocationAsync -> {ex.Message}");
                return new Vector3();
            }
        }

        public async Task TestQueryAsync()
        {
            await mySql.Query("SELECT 1;");
        }
    }
}
