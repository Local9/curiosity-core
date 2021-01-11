﻿using CitizenFX.Core.Native;
using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Models;
using GHMatti.Data.MySQL.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Database.Store
{
    class CharacterDatabase
    {
        public static async Task<CuriosityCharacter> Get(ulong discordId)
        {
            int serverId = PluginManager.ServerId;
            int starterCash = API.GetConvarInt("starter_cash", 100);

            Dictionary<string, object> myParams = new Dictionary<string, object>()
            {
                { "@DiscordID", discordId },
                { "@ServerID", serverId }
            };

            string myQuery = "CALL spGetCharacter(@DiscordID, @ServerID);";

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                if (keyValuePairs.Count == 0)
                    return null;

                CuriosityCharacter curiosityCharacter = new CuriosityCharacter();

                foreach (Dictionary<string, object> kv in keyValuePairs)
                {
                    if (kv.ContainsKey("CharacterJSON") && kv["CharacterJSON"] != null)
                        curiosityCharacter = JsonConvert.DeserializeObject<CuriosityCharacter>($"{kv["CharacterJSON"]}");

                    curiosityCharacter.CharacterId = kv["characterId"].ToLong();
                    curiosityCharacter.MarkedAsRegistered = kv["IsRegistered"].ToBoolean();
                    curiosityCharacter.Cash = kv["Cash"].ToLong();

                    if (!curiosityCharacter.MarkedAsRegistered)
                        curiosityCharacter.Cash = starterCash;
                }

                return curiosityCharacter;
            }
        }

        public static async Task Save(CuriosityCharacter curiosityCharacter)
        {
            if (!curiosityCharacter.MarkedAsRegistered) return;

            string characterJson = JsonConvert.SerializeObject(curiosityCharacter);

            Dictionary<string, object> myParams = new Dictionary<string, object>()
            {
                { "@CharacterId", curiosityCharacter.CharacterId },
                { "@IsRegistered", curiosityCharacter.MarkedAsRegistered },
                { "@CharacterJSON", characterJson }
            };

            string myQuery = "CALL upCharacter(@CharacterId, @IsRegistered, @CharacterJSON);";

            await MySqlDatabase.mySQL.Query(myQuery, myParams);
        }
    }
}