using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Extensions;
using GHMatti.Data.MySQL.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
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
                    if (kv.ContainsKey("characterObject") && kv["characterObject"] != null)
                        curiosityCharacter = JsonConvert.DeserializeObject<CuriosityCharacter>($"{kv["characterObject"]}");

                    curiosityCharacter.CharacterId = kv["characterId"].ToLong();
                    curiosityCharacter.MarkedAsRegistered = kv["registered"].ToBoolean();
                    curiosityCharacter.Cash = kv["cash"].ToLong();
                    curiosityCharacter.IsDead = kv["dead"].ToBoolean();

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
                { "@CharacterIdent", curiosityCharacter.CharacterId },
                { "@CashAmount", curiosityCharacter.Cash },
                { "@IsRegistered", curiosityCharacter.MarkedAsRegistered },
                { "@IsDead", curiosityCharacter.Health == 0 },
                { "@CharacterJson", characterJson }
            };

            string myQuery = "CALL curiosity.upCharacter(@CharacterIdent, @CashAmount, @IsRegistered, @IsDead, @CharacterJson);";

            await MySqlDatabase.mySQL.Query(myQuery, myParams);
        }
    }
}
