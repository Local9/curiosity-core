﻿using Curiosity.Framework.Server.Models;
using Dapper;
using Logger;
using MySqlConnector;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Debug = CitizenFX.Core.Debug;

namespace Curiosity.Framework.Server.Database
{
    internal class DapperDatabase<T>
    {
        private static string _connectionString;

        private static string ConnectionString()
        {
            if (!string.IsNullOrEmpty(_connectionString))
                return _connectionString;

            DatabaseConfig databaseConfig = ServerConfiguration.GetDatabaseConfig;

            MySqlConnectionStringBuilder mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder();
            mySqlConnectionStringBuilder.ApplicationName = "Curiosity";

            mySqlConnectionStringBuilder.Database = databaseConfig.DatabaseName;
            mySqlConnectionStringBuilder.Server = databaseConfig.Server;
            mySqlConnectionStringBuilder.Port = databaseConfig.Port;
            mySqlConnectionStringBuilder.UserID = databaseConfig.Username;
            mySqlConnectionStringBuilder.Password = databaseConfig.Password;

            mySqlConnectionStringBuilder.MaximumPoolSize = databaseConfig.MaximumPoolSize;
            mySqlConnectionStringBuilder.MinimumPoolSize = databaseConfig.MinimumPoolSize;
            mySqlConnectionStringBuilder.ConnectionTimeout = databaseConfig.ConnectionTimeout;

            return _connectionString = mySqlConnectionStringBuilder.ToString();
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
            Debug.WriteLine($"{Log.DARK_RED}{sb}");
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
