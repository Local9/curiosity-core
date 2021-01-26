﻿using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Models;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Database.Store
{
    class StatDatabase
    {
        public static async Task<int> Adjust(int characterId, int skillId, int amount)
        {
            Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@characterId", characterId },
                    { "@statId", skillId },
                    { "@amount", amount },
                };

            string myQuery = "call upCharacterStat(@characterId, @statId, @amount);";

            int newValue = 0;

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                if (keyValuePairs.Count == 0)
                    throw new Exception("Stat was not changed");

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

                if (keyValuePairs.Count == 0)
                    return lst;

                foreach (Dictionary<string, object> kv in keyValuePairs)
                {
                    CharacterStat characterStat = new CharacterStat();
                    characterStat.Label = $"{kv["Label"]}";
                    characterStat.Value = kv["Value"].ToLong();
                    lst.Add(characterStat);
                }
            }

            return lst;
        }
    }
}