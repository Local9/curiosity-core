﻿using GHMatti.Data.MySQL;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CitizenFX.Core;

namespace Curiosity.Server.net.Database
{
    public class DatabaseUsers : BaseScript
    {
        static MySQL mySql;

        public static void Init()
        {
            mySql = Database.mySQL;
        }

        public static async Task<Entity.User> GetUserAsync(string license)
        {
            try
            {
                Entity.User user = new Entity.User();
                user.BankAccount = CitizenFX.Core.Native.API.GetConvarInt("starter_bank", 1000);
                user.Wallet = CitizenFX.Core.Native.API.GetConvarInt("starter_cash", 100);
                user.LocationId = CitizenFX.Core.Native.API.GetConvarInt("starting_location_id", 1);
                user.ServerId = Server.serverId;

                Dictionary<string, object> myParams = new Dictionary<string, object>();
                myParams.Add("@license", license);
                myParams.Add("@serverId", user.ServerId);
                myParams.Add("@starterCash", user.Wallet);
                myParams.Add("@starterBank", user.BankAccount);
                myParams.Add("@locationId", user.LocationId);

                string selectQuery = "CALL `curiosity`.`spGetUserCharacter`(@license, @serverId, @starterCash, @starterBank, @locationId);";

                using (var result = mySql.QueryResult(selectQuery, myParams))
                {
                    ResultSet keyValuePairs = await result;
                    await Delay(0);
                    if (keyValuePairs.Count == 0)
                    {
                        throw new Exception("SQL ERROR");
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

                        user.PosX = int.Parse($"{keyValues["x"]}");
                        user.PosY = int.Parse($"{keyValues["y"]}");
                        user.PosZ = int.Parse($"{keyValues["z"]}");

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
                Debug.WriteLine($"GetUserIdAsync -> {ex.Message}");
                return null;
            }
        }

        public static void IncreaseLiveExperience(long userId, int experience)
        {
            string query = "update user set `lifeExperience` = `lifeExperience` + @experience where userId = @userId";

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@userId", userId);
            myParams.Add("@experience", experience);

            mySql.Query(query, myParams);
        }

        public static void DecreaseLiveExperience(long userId, int experience)
        {
            string query = "update user set `lifeExperience` = `lifeExperience` - @experience where userId = @userId";

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
                myParams.Add("@z", z);

                string selectQuery = "update location set x = @x, y = @y, z = @z where locationId = @locationId";
                await mySql.Query(selectQuery, myParams, false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdatePlayerLocation -> {ex.Message}");
            }
        }

        public static async Task UpdateUserLocationId(long userId, long locationId)
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

        public static async Task<long> SaveLocationAsync(int locationType, float x, float y, float z)
        {
            try
            {
                Dictionary<string, object> myParams = new Dictionary<string, object>();
                myParams.Add("@locationType", locationType);
                myParams.Add("@x", x);
                myParams.Add("@y", y);
                myParams.Add("@z", z);
                myParams.Add("@serverId", Server.serverId);

                string selectQuery = "insert into locations (locationType, locationDescription, x, y, z) values (@locationType, 'Player', @x, @y, @z)";
                return await mySql.Query(selectQuery, myParams, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveLocationAsync -> {ex.Message}");
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

        public static async Task TestQueryAsync()
        {
            await mySql.Query("SELECT 1;");
        }
    }
}
