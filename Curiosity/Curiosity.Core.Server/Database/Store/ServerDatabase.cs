using Curiosity.Core.Server.Diagnostics;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Database.Store
{
    class ServerDatabase
    {
        public static async Task<bool> CheckServerKey(int serverId)
        {
            try
            {
                Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@ServerID", serverId }
                };

                string myQuery = "CALL selServer(@ServerID);";

                using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
                {
                    ResultSet keyValuePairs = await result;

                    if (keyValuePairs.Count == 0)
                        return false;

                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
                return false;
            }
        }
    }
}
