using CitizenFX.Core;
using Curiosity.Core.Server.Extensions;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Database.Store
{
    class BankDatabase
    {
        public static async Task<ulong> Adjust(int characterId, long amount)
        {
            Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@characterId", characterId },
                    { "@amount", amount },
                };

            string myQuery = "call upCharacterCash(@characterId, @amount);";

            ulong newValue = 0;

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                await BaseScript.Delay(0);

                if (keyValuePairs.Count == 0)
                    throw new Exception("Character cash value not changed");

                foreach (Dictionary<string, object> kv in keyValuePairs)
                {
                    newValue = kv["return"].ToUnsignedLong();
                }
            }

            return newValue;
        }

        public static async Task<ulong> Get(int characterId)
        {
            Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@characterId", characterId }
                };

            string myQuery = "call selCharacterCash(@characterId);";

            ulong newValue = 0;

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                await BaseScript.Delay(0);

                if (keyValuePairs.Count == 0)
                    throw new Exception("Character cash value not changed");

                foreach (Dictionary<string, object> kv in keyValuePairs)
                {
                    newValue = kv["return"].ToUnsignedLong();
                }
            }

            return newValue;
        }
    }
}
