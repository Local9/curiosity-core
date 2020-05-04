using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Diagnostics;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Curiosity.Systems.Server.Database.Store
{
    class CharacterDatabase
    {
        public static async Task<CuriosityCharacter> Get(ulong discordId)
        {
            int serverId = CuriosityPlugin.ServerId;
            int starterCash = API.GetConvarInt("starter_cash", 100);

            Dictionary<string, object> myParams = new Dictionary<string, object>()
            {
                { "@discordIdent", discordId },
                { "@serverIdent", serverId }
            };

            string myQuery = "CALL curiosity.spGetUserCharacter_v2(@discordIdent, @serverIdent);";

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                if (keyValuePairs.Count == 0)
                    return null;

                CuriosityCharacter curiosityCharacter = new CuriosityCharacter();

                foreach (Dictionary<string, object> kv in keyValuePairs)
                {
                    if (kv.ContainsValue("characterObject"))
                        curiosityCharacter = Newtonsoft.Json.JsonConvert.DeserializeObject<CuriosityCharacter>($"{kv["characterObject"]}");

                    curiosityCharacter.CharacterId = (long)kv["characterId"];
                    curiosityCharacter.MarkedAsRegistered = (bool)kv["registered"];
                    curiosityCharacter.Cash = (long)kv["cash"];
                    curiosityCharacter.IsDead = (bool)kv["dead"];

                    if (!curiosityCharacter.MarkedAsRegistered)
                        curiosityCharacter.Cash = starterCash;
                }

                return curiosityCharacter;
            }
        }

        public static async Task Save(CuriosityCharacter curiosityCharacter)
        {
            string characterJson = Newtonsoft.Json.JsonConvert.SerializeObject(curiosityCharacter);

            Dictionary<string, object> myParams = new Dictionary<string, object>()
            {
                { "@CharacterIdent", curiosityCharacter.CharacterId },
                { "@CashAmount", curiosityCharacter.Cash },
                { "@IsRegistered", curiosityCharacter.MarkedAsRegistered },
                { "@IsDead", curiosityCharacter.Health == 0 },
                { "@CharacterJson", characterJson }
            };

            string myQuery = "CALL curiosity.upCharacter(@license, @username, @discordId);";

            await MySqlDatabase.mySQL.Query(myQuery, myParams);
        }
    }
}
