using Curiosity.Core.Server.Extensions;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Database.Store
{
    class BankDatabase
    {
        public static async Task<int> Adjust(int characterId, int amount)
        {
            Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@characterId", characterId },
                    { "@amount", amount },
                };

            string myQuery = "call upCharacterCash(@characterId, @amount);";

            int newValue = 0;

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                if (keyValuePairs.Count == 0)
                    throw new Exception("Character cash value not changed");

                foreach (Dictionary<string, object> kv in keyValuePairs)
                {
                    newValue = kv["return"].ToInt();
                }
            }

            return newValue;
        }
    }
}
