using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Curiosity.LifeV.Bot.Models
{
    public class Log
    {
        public DateTime Timestamp;
        public string Details;
        public string LoggedBy;

        public Log() { }

        public async Task<List<Log>> GetUserLogAsync(long userId)
        {
            try
            {
                using var connection = await Database.DatabaseConfig.GetDatabaseConnection();
                await connection.OpenAsync();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"call selUserLog(null, @identifer);"; // userId only
                cmd.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@identifer",
                    DbType = DbType.Int64,
                    Value = userId
                });
                var result = await ReadAllAsync(await cmd.ExecuteReaderAsync());
                return result.Count > 0 ? result : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUserLogAsync -> {ex}");
                return default;
            }
        }

        private async Task<List<Log>> ReadAllAsync(DbDataReader reader)
        {
            var logs = new List<Log>();
            using (reader)
            {
                while (await reader.ReadAsync())
                {
                    var log = new Log()
                    {
                        Timestamp = reader.GetFieldValue<DateTime>("timestamp"),
                        Details = reader.GetFieldValue<string>("details"),
                        LoggedBy = reader.GetFieldValue<string>("username"),
                    };

                    logs.Add(log);
                }
            }
            return logs;
        }
    }
}
