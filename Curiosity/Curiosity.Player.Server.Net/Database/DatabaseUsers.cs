using CitizenFX.Core;
using Curiosity.Global.Shared.net.Enums;
using Curiosity.Shared.Server.net.Helpers;
using GHMatti.Data.MySQL;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Server.net.Database
{
    class DatabaseUsers : BaseScript
    {
        static MySQL mySql;

        public static void Init()
        {
            mySql = Database.mySQL;
        }

        public static async Task<Entity.User> GetUser(string license, Player player)
        {
            Entity.User user = new Entity.User();
            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@license", license);
            myParams.Add("@username", player.Name);

            string selectQuery = "CALL curiosity.spGetUser(@license, @username);";

            using (var result = mySql.QueryResult(selectQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                await Delay(200);

                if (keyValuePairs.Count == 0)
                {
                    player.Drop($"Sorry {player.Name}, an error occurred while you were trying to connect to the server or update your characters information, please try to connect again. If the issue persists visit our Discord @ discord.gg/6xHuXwG");
                    throw new Exception($"SQL ERROR -> No rows returned : Maybe failed to setup the character {player.Name}");
                }

                foreach (Dictionary<string, object> keyValues in keyValuePairs)
                {
                    user.UserId = int.Parse($"{keyValues["userId"]}");
                    user.LifeExperience = int.Parse($"{keyValues["lifeExperience"]}");
                    user.DateCreated = DateTime.Parse($"{keyValues["dateCreated"]}");
                    string bannedUntilDate = $"{keyValues["bannedUntil"]}";
                    user.BannedUntil = (!string.IsNullOrEmpty(bannedUntilDate)) ? DateTime.Parse($"{keyValues["bannedUntil"]}").ToString("yyyy-MM-dd HH:mm") : string.Empty;
                    user.BannedPerm = ($"{keyValues["bannedPerm"]}" == "1");
                    user.Banned = ($"{keyValues["banned"]}" == "1");
                    user.QueueLevel = int.Parse($"{keyValues["queueLevel"]}");
                    user.QueuePriority = int.Parse($"{keyValues["queuePriority"]}");
                }
                return user;
            }
        }

        internal static void UpdateCharacterRole(int characterId, Privilege privilege)
        {
            string query = "CALL curiosity.upCharacterRole(@characterId, @roleId);";
            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@characterId", characterId);
            myParams.Add("@roleId", (int)privilege);
            mySql.Query(query, myParams);
        }

        public static async Task<Entity.User> GetUserWithCharacterAsync(string license, Player player)
        {
            try
            {
                Entity.User user = new Entity.User();
                user.BankAccount = CitizenFX.Core.Native.API.GetConvarInt("starter_bank", 1000);
                user.Wallet = CitizenFX.Core.Native.API.GetConvarInt("starter_cash", 100);
                user.LocationId = Server.startingLocationId;
                user.ServerId = Server.serverId;

                Dictionary<string, object> myParams = new Dictionary<string, object>();
                myParams.Add("@license", license);
                myParams.Add("@serverId", user.ServerId);
                myParams.Add("@starterCash", user.Wallet);
                myParams.Add("@starterBank", user.BankAccount);
                myParams.Add("@locationId", user.LocationId);

                string selectQuery = "CALL curiosity.spGetUserCharacter(@license, @serverId, @starterCash, @starterBank, @locationId);";

                using (var result = mySql.QueryResult(selectQuery, myParams))
                {
                    ResultSet keyValuePairs = await result;

                    await Delay(200);

                    if (keyValuePairs.Count == 0)
                    {
                        player.Drop($"Sorry {player.Name}, an error occurred while you were trying to connect to the server or update your characters information, please try to connect again. If the issue persists visit our Discord @ discord.gg/6xHuXwG");
                        throw new Exception($"SQL ERROR -> No rows returned : Maybe failed to setup the character {player.Name}");
                    }

                    foreach (Dictionary<string, object> keyValues in keyValuePairs)
                    {
                        user.UserId = int.Parse($"{keyValues["userId"]}");
                        user.LifeExperience = int.Parse($"{keyValues["lifeExperience"]}");
                        user.DateCreated = DateTime.Parse($"{keyValues["dateCreated"]}");

                        user.CharacterId = int.Parse($"{keyValues["characterId"]}");
                        user.LocationId = int.Parse($"{keyValues["locationId"]}");

                        user.RoleId = int.Parse($"{keyValues["roleId"]}");
                        user.Role = $"{keyValues["roleDescription"]}";

                        user.StatId = int.Parse($"{keyValues["statId"]}");

                        user.BankId = int.Parse($"{keyValues["bankId"]}");
                        user.Wallet = int.Parse($"{keyValues["wallet"]}");
                        user.BankAccount = int.Parse($"{keyValues["bankAccount"]}");

                        user.PosX = float.Parse($"{keyValues["x"]}");
                        user.PosY = float.Parse($"{keyValues["y"]}");
                        user.PosZ = float.Parse($"{keyValues["z"]}");

                        user.Stamina = int.Parse($"{keyValues["stamina"]}");
                        user.Strength = int.Parse($"{keyValues["strength"]}");
                        user.Stealth = int.Parse($"{keyValues["stealth"]}");
                        user.Flying = int.Parse($"{keyValues["flying"]}");
                        user.Driving = int.Parse($"{keyValues["driving"]}");
                        user.LungCapacity = int.Parse($"{keyValues["lungCapacity"]}");
                        user.MentalState = int.Parse($"{keyValues["mentalState"]}");
                    }
                    return user;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"GetUserWithCharacterAsync() -> {ex.Message}");
                return null;
            }
        }

        public static void LogReport(int userId, int userIdLogging, int logTypeId, int characterId)
        {
            string query = "call spLogReportUser(@userId, @loggedById, @logTypeId, @characterId);";

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@userId", userId);
            myParams.Add("@loggedById", userIdLogging);
            myParams.Add("@logTypeId", logTypeId);
            myParams.Add("@characterId", characterId);

            mySql.Query(query, myParams);
        }

        public static void LogKick(int userId, int userIdLogging, int logTypeId, int characterId)
        {
            string query = "call spLogKickedUser(@userId, @loggedById, @logTypeId, @characterId);";

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@userId", userId);
            myParams.Add("@loggedById", userIdLogging);
            myParams.Add("@logTypeId", logTypeId);
            myParams.Add("@characterId", characterId);

            mySql.Query(query, myParams);
        }

        public static void LogBan(int userId, int userIdLogging, int logTypeId, int characterId, bool permBan, DateTime bannedUntil)
        {
            string query = "call spLogBannedUser(@userId, @loggedById, @logTypeId, @characterId, @permBan, @bannedUntil);";

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@userId", userId);
            myParams.Add("@loggedById", userIdLogging);
            myParams.Add("@logTypeId", logTypeId);
            myParams.Add("@characterId", characterId);
            myParams.Add("@permBan", permBan);
            myParams.Add("@bannedUntil", bannedUntil);

            mySql.Query(query, myParams);
        }

        public static void IncreaseLiveExperience(long userId, int experience)
        {
            string query = "update curiosity.user set `lifeExperience` = `lifeExperience` + @experience where userId = @userId;";

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@userId", userId);
            myParams.Add("@experience", experience);

            mySql.Query(query, myParams);
        }

        public static void DecreaseLiveExperience(long userId, int experience)
        {
            string query = "update curiosity.user set `lifeExperience` = `lifeExperience` - @experience where userId = @userId;";

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@userId", userId);
            myParams.Add("@experience", experience);

            mySql.Query(query, myParams);
        }

        public static async Task UpdatePlayerLocation(long locationId, float x, float y, float z)
        {
            try
            {
                Dictionary<string, object> myParams = new Dictionary<string, object>();
                myParams.Add("@locationId", locationId);
                myParams.Add("@x", x);
                myParams.Add("@y", y);
                myParams.Add("@z", z + 1.0f);

                string selectQuery = "update curiosity.location set x = @x, y = @y, z = @z where locationId = @locationId;";
                await mySql.Query(selectQuery, myParams, false);
            }
            catch (Exception ex)
            {
                Log.Error($"UpdatePlayerLocation -> {ex.Message}");
            }
        }

        public static async Task UpdateUserLocationId(long characterId, long locationId)
        {
            try
            {
                Dictionary<string, object> myParams = new Dictionary<string, object>();
                myParams.Add("@characterId", characterId);
                myParams.Add("@locationId", locationId);

                string selectQuery = "update curiosity.character set locationId = @locationId where characterId = @characterId;";
                await mySql.Query(selectQuery, myParams, false);
            }
            catch (Exception ex)
            {
                Log.Error($"UpdateUserLocationId -> {ex.Message}");
            }
        }

        public static async Task<long> SaveLocationAsync(int locationType, float x, float y, float z)
        {
            try
            {
                Dictionary<string, object> myParams = new Dictionary<string, object>();
                myParams.Add("@locationType", locationType);
                myParams.Add("@x", x);
                myParams.Add("@y", y);
                myParams.Add("@z", z + 1.0f);
                myParams.Add("@serverId", Server.serverId);

                string selectQuery = "insert into curiosity.location (locationTypeId, description, x, y, z, serverId) values (@locationType, 'Character Spawn', @x, @y, @z, @serverId);";
                return await mySql.Query(selectQuery, myParams, true);
            }
            catch (Exception ex)
            {
                Log.Error($"SaveLocationAsync -> {ex.Message}");
                return 0;
            }
        }

        public static async Task<Vector3> GetUserLocationAsync(long locationId)
        {
            try
            {
                Vector3 vector3 = new Vector3();

                Dictionary<string, object> myParams = new Dictionary<string, object>();
                myParams.Add("@locationId", locationId);
                string selectQuery = "select x, y, z from curiosity.location where locationId = @locationId;";
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
                Log.Error($"GetUserLocationAsync -> {ex.Message}");
                return new Vector3();
            }
        }

        public static async Task TestQueryAsync()
        {
            await mySql.Query("SELECT 1;");
        }
    }
}
