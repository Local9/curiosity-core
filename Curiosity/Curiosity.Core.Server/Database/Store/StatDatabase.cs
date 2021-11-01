using CitizenFX.Core;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using GHMatti.Data.MySQL.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Database.Store
{
    class StatDatabase
    {
        public static async Task<int> Adjust(int characterId, Stat statId, object amount)
        {
            Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@characterId", characterId },
                    { "@statId", statId },
                    { "@amount", amount },
                };

            string myQuery = "call upCharacterStat(@characterId, @statId, @amount);";

            int newValue = 0;

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                await BaseScript.Delay(0);

                if (keyValuePairs.Count == 0)
                {
                    Logger.Error($"{statId} not changed");
                    return 0;
                }

                foreach (Dictionary<string, object> kv in keyValuePairs)
                {
                    newValue = kv["return"].ToInt();
                }
            }

            return newValue;
        }

        public static async Task<List<CharacterStat>> Get(int characterId)
        {
            Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@characterId", characterId },
                };

            string myQuery = "call selCharacterStats(@characterId);";

            List<CharacterStat> lst = new List<CharacterStat>();

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                await BaseScript.Delay(0);

                if (keyValuePairs.Count == 0)
                    return lst;

                foreach (Dictionary<string, object> kv in keyValuePairs)
                {
                    CharacterStat characterStat = new CharacterStat();
                    characterStat.Id = kv["StatId"].ToInt();
                    characterStat.Label = $"{kv["Label"]}";
                    characterStat.Value = kv["Value"].ToLong();
                    lst.Add(characterStat);
                }
            }

            return lst;
        }

        public static async Task<int> Get(int characterId, Stat stat)
        {
            Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@characterId", characterId },
                    { "@statId", (int)stat }
                };

            string myQuery = "call selCharacterStat(@characterId, @statId);";

            int newValue = 0;

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                await BaseScript.Delay(0);

                if (keyValuePairs.Count == 0)
                    return newValue; // nothing found, return zero

                foreach (Dictionary<string, object> kv in keyValuePairs)
                {
                    newValue = kv["return"].ToInt();
                }
            }

            return newValue;
        }
    }
}
