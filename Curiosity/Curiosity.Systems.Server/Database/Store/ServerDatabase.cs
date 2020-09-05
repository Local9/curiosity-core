using CitizenFX.Core;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Diagnostics;
using Curiosity.Systems.Server.Extensions;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Systems.Server.Database.Store
{
    class ServerDatabase
    {
        public static async Task<bool> CheckServerKey(int serverId, string serverKey)
        {
            try
            {
                Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@ServerID", serverId },
                    { "@ServerKey", serverKey }
                };

                string myQuery = "CALL selServer(@ServerID, @ServerKey);";

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
