﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Diagnostics;
using MySql.Data.MySqlClient;
using System.Data;
using System.Threading.Tasks;

namespace Curiosity.Systems.Server.MySQL.Store
{
    class CharacterDatabase
    {
        public static async Task<CuriosityCharacter> Get(ulong discordId)
        {
            int serverId = CuriosityPlugin.ServerId;
            int starterCash = API.GetConvarInt("starter_cash", 100);

            using (var db = new MySqlDatabase())
            {
                await db.Connection.OpenAsync();

                using (var cmd = db.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "spGetUserCharacter_v2";
                    cmd.Parameters.AddWithValue("@discordIdent", discordId);
                    cmd.Parameters.AddWithValue("@serverIdent", serverId);


                    foreach (MySqlParameter param in cmd.Parameters)
                    {
                        Logger.Debug($"{param.ParameterName} = {param.Value}");
                    }

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            CuriosityCharacter curiosityCharacter = new CuriosityCharacter();

                            if (reader.HasRows)
                            {
                                if (!reader.IsDBNull(4))
                                    curiosityCharacter = Newtonsoft.Json.JsonConvert.DeserializeObject<CuriosityCharacter>(reader.GetString(4));

                                curiosityCharacter.CharacterId = reader.GetInt64(0);
                                curiosityCharacter.MarkedAsRegistered = reader.GetBoolean(1);
                            }

                            if (!curiosityCharacter.MarkedAsRegistered)
                                curiosityCharacter.Cash = starterCash;

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
                    cmd.CommandText = "upCharacter";
                    cmd.Parameters.AddWithValue("@CharacterIdent", curiosityCharacter.CharacterId);
                    cmd.Parameters.AddWithValue("@CashAmount", curiosityCharacter.Cash);
                    cmd.Parameters.AddWithValue("@IsRegistered", curiosityCharacter.MarkedAsRegistered);
                    cmd.Parameters.AddWithValue("@IsDead", curiosityCharacter.Health == 0);
                    cmd.Parameters.AddWithValue("@CharacterJson", characterJson);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
