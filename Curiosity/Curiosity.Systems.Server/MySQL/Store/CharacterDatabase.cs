using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Server.MySQL.Store
{
    class CharacterDatabase
    {
        public static async Task<CuriosityCharacter> Get(string license, Player player)
        {
            int serverId = CuriosityPlugin.ServerId;
            int starterCash = API.GetConvarInt("starter_cash", 100);
            int starterBank = API.GetConvarInt("starter_bank", 1000);
            int serverSpawnLocationId = CuriosityPlugin.SpawnLocationId;

            using (var db = new MySqlDatabase())
            {
                await db.Connection.OpenAsync();

                using (var cmd = db.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "spGetUserCharacter";
                    cmd.Parameters.AddWithValue("@licenseIn", license);
                    cmd.Parameters.AddWithValue("@serverIdIn", serverId);
                    cmd.Parameters.AddWithValue("@starterCash", starterCash);
                    cmd.Parameters.AddWithValue("@starterBankAccount", starterBank);
                    cmd.Parameters.AddWithValue("@locationIdIn", serverSpawnLocationId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if (!reader.HasRows)
                            {
                                player.Drop($"Sorry {player.Name}, an error occurred while you were trying to connect to the server or update your characters information, please try to connect again. If the issue persists visit our Discord @ {CuriosityPlugin.DiscordUrl}");
                                throw new Exception($"SQL ERROR -> Failed to find information for {player.Name} [{player.Identifiers["license"]}]");
                            }

                            CuriosityCharacter curiosityCharacter = new CuriosityCharacter()
                            {
                                CharacterId = reader.GetInt64(3),
                                LocationId = reader.GetInt64(4),
                                MarkedAsRegistered = false
                            };

                            curiosityCharacter.LastPosition = new Position(reader.GetFloat(11), reader.GetFloat(12), reader.GetFloat(13));
                            curiosityCharacter.Style = new CharacterHeritage();

                            return curiosityCharacter;
                        }
                    }
                }
            }
            return null;
        }

        public static async Task Save(CuriosityCharacter curiosityCharacter)
        {
            string characterJson = Newtonsoft.Json.JsonConvert.SerializeObject(curiosityCharacter);

            using (var db = new MySqlDatabase())
            {
                await db.Connection.OpenAsync();
                using (var cmd = db.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "upCharacterSkin";
                    cmd.Parameters.AddWithValue("@characterIdIn", curiosityCharacter.CharacterId);
                    cmd.Parameters.AddWithValue("@skinIn", characterJson);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
