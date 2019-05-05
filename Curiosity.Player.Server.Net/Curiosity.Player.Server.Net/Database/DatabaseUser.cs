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

        public async Task<long> GetUserIdAsync(string steamId)
        {
            try
            {
                long userId = 0;

                Dictionary<string, object> myParams = new Dictionary<string, object>();
                myParams.Add("@steamId", steamId);
                string selectQuery = "select userId from users where steamId = @steamId";
                using (var result = mySql.QueryResult(selectQuery, myParams))
                {
                    ResultSet keyValuePairs = await result;
                    await Delay(0);
                    if (keyValuePairs.Count == 0)
                    {
                        string myInsertQuery = "insert into users (steamId) values (@steamId);";
                        userId = await mySql.Query(myInsertQuery, myParams, true);
                        await Delay(0);
                    }

                    foreach (Dictionary<string, object> keyValues in keyValuePairs)
                    {
                        userId = long.Parse($"{keyValues["userId"]}");
                    }
                    return userId;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetUserIdAsync -> {ex.Message}");
                return 0;
            }
        }

        public async Task TestQueryAsync()
        {
            await mySql.Query("SELECT 1;");
        }
    }
}
