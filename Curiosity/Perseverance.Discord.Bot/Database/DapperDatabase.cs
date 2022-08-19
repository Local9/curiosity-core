﻿using Dapper;
using MySqlConnector;
using Perseverance.Discord.Bot.Entities;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Perseverance.Discord.Bot.Database
{
    internal class DapperDatabase<T>
    {
        private static string _connectionString;

        private static string ConnectionString()
        {
            if (!string.IsNullOrEmpty(_connectionString))
                return _connectionString;

            DatabaseInstance databaseConfig = Program.Configuration.DatabaseInstance;

            MySqlConnectionStringBuilder mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder();
            mySqlConnectionStringBuilder.ApplicationName = "Perseverance-Discord-Bot";

            mySqlConnectionStringBuilder.Database = databaseConfig.Database;
            mySqlConnectionStringBuilder.Server = databaseConfig.Server;
            mySqlConnectionStringBuilder.Port = databaseConfig.Port;
            mySqlConnectionStringBuilder.UserID = databaseConfig.Username;
            mySqlConnectionStringBuilder.Password = databaseConfig.Password;

            //mySqlConnectionStringBuilder.MaximumPoolSize = databaseConfig.MaximumPoolSize;
            //mySqlConnectionStringBuilder.MinimumPoolSize = databaseConfig.MinimumPoolSize;
            //mySqlConnectionStringBuilder.ConnectionTimeout = databaseConfig.ConnectionTimeout;

            return _connectionString = $"{mySqlConnectionStringBuilder}";
        }

        public static async Task<List<T>> GetListAsync(string query, DynamicParameters args = null)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                using (var conn = new MySqlConnection(ConnectionString()))
                {
                    SetupTypeMap();

                    return (await conn.QueryAsync<T>(query, args)).AsList();
                }
            }
            catch (Exception ex)
            {
                SqlExceptionHandler(query, ex.Message, watch.ElapsedMilliseconds);
            }
            finally
            {
                watch.Stop();
            }
            return null;
        }

        public static async Task<Dictionary<string, string>> GetDictionaryAsync(string query, DynamicParameters args = null)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                using (var conn = new MySqlConnection(ConnectionString()))
                {
                    SetupTypeMap();

                    return (await conn.QueryAsync<KeyValuePair<string, string>>(query, args)).ToDictionary(pair => pair.Key, pair => pair.Value);
                }
            }
            catch (Exception ex)
            {
                SqlExceptionHandler(query, ex.Message, watch.ElapsedMilliseconds);
            }
            finally
            {
                watch.Stop();
            }
            return null;
        }

        public static async Task<T> GetSingleAsync(string query, DynamicParameters args = null)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                using (var conn = new MySqlConnection(ConnectionString()))
                {
                    SetupTypeMap();
                    return (await conn.QueryAsync<T>(query, args)).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                SqlExceptionHandler(query, ex.Message, watch.ElapsedMilliseconds);
            }
            finally
            {
                watch.Stop();
            }
            return default(T);
        }

        public static async Task<bool> ExecuteAsync(string query, DynamicParameters args = null)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                using (var conn = new MySqlConnection(ConnectionString()))
                {
                    return (await conn.ExecuteAsync(query, args)) > 0;
                }
            }
            catch (Exception ex)
            {
                SqlExceptionHandler(query, ex.Message, watch.ElapsedMilliseconds);
            }
            finally
            {
                watch.Stop();
            }
            return false;
        }

        private static void SqlExceptionHandler(string query, string exceptionMessage, long elapsedMilliseconds)
        {
            StringBuilder sb = new();
            sb.Append("** SQL Exception **\n");
            sb.Append($"Query: {query}\n");
            sb.Append($"Exception Message: {exceptionMessage}\n");
            sb.Append($"Time Elapsed: {elapsedMilliseconds}ms");
            Program.SendMessage(Program.BOT_TEXT_CHANNEL, sb.ToString());
        }

        private static void SetupTypeMap()
        {
            var map = new CustomPropertyTypeMap(typeof(T), (type, columnName) =>
                                type.GetProperties().FirstOrDefault(prop => GetDescriptionFromAttribute(prop) == columnName.ToLower()));
            SqlMapper.SetTypeMap(typeof(T), map);
        }

        public static string GetDescriptionFromAttribute(MemberInfo member)
        {
            if (member == null) return null;

            var attrib = (DescriptionAttribute)Attribute.GetCustomAttribute(member, typeof(DescriptionAttribute), false);
            return (attrib?.Description ?? member.Name).ToLower();
        }
    }
}
