﻿using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Models;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Database.Store
{
    class SkillDatabase
    {
        public static async Task<int> Adjust(int characterId, int skillId, int amount)
        {
            Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@characterId", characterId },
                    { "@skillId", skillId },
                    { "@amount", amount },
                };

            string myQuery = "call upCharacterSkill(@characterId, @skillId, @amount);";

            int newValue = 0;

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                if (keyValuePairs.Count == 0)
                    throw new Exception("Skill was not changed");

                foreach(Dictionary<string, object> kv in keyValuePairs)
                {
                    newValue = kv["return"].ToInt();
                }
            }

            return newValue;
        }

        public static async Task<List<CharacterSkill>> Get(int characterId)
        {
            Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@characterId", characterId },
                };

            string myQuery = "call selCharacterSkills(@characterId);";

            List<CharacterSkill> lst = new List<CharacterSkill>();

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                if (keyValuePairs.Count == 0)
                    return lst;

                foreach (Dictionary<string, object> kv in keyValuePairs)
                {
                    CharacterSkill characterSkill = new CharacterSkill();
                    characterSkill.Label = $"{kv["Label"]}";
                    characterSkill.Description = $"{kv["Description"]}";
                    characterSkill.Value = kv["Value"].ToLong();
                    lst.Add(characterSkill);
                }
            }

            return lst;
        }
    }
}