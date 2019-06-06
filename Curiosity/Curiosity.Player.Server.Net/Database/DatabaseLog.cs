using CitizenFX.Core;
using GHMatti.Data.MySQL;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GlobalEntities = Curiosity.Global.Shared.net.Entity;
using GlobalEnums = Curiosity.Global.Shared.net.Enums;

namespace Curiosity.Server.net.Database
{
    class DatabaseLog
    {
        static MySQL mySql;

        public static void Init()
        {
            mySql = Database.mySQL;
        }

        public static async Task<List<GlobalEntities.LogType>> GetLogReasons(GlobalEnums.LogGroup logGroup)
        {
            List<GlobalEntities.LogType> lst = new List<GlobalEntities.LogType>();
            try
            {
                Dictionary<string, object> myParams = new Dictionary<string, object>();
                myParams.Add("@logGroupId", (int)logGroup);

                string selectQuery = "call selLogReasons(@logGroupId);";
                using (var results = mySql.QueryResult(selectQuery, myParams))
                {
                    ResultSet keyValuePairs = await results;
                    await BaseScript.Delay(10);
                    foreach (Dictionary<string, object> keyValues in keyValuePairs)
                    {
                        GlobalEntities.LogType logType = new GlobalEntities.LogType
                        {
                            LogTypeId = int.Parse($"{keyValues["logTypeId"]}"),
                            Group = $"{keyValues["group"]}",
                            Description = $"{keyValues["reason"]}"
                        };
                        lst.Add(logType);
                    }
                    return lst;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveLocationAsync -> {ex.Message}");
                return lst;
            }
        }
    }
}
