using CitizenFX.Core;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Diagnostics;
using Curiosity.Systems.Server.MySQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Server.Managers
{
    public class UserManager : Manager<UserManager>
    {
        public override void Begin()
        {
            
        }

        public async Task<CuriosityUser> Get(string license, Player player, ulong discordId)
        {
            CuriosityUser user = new CuriosityUser();
            user.Character = new CuriosityCharacter();

            using (var db = new MySqlDatabase())
            {
                await db.Connection.OpenAsync();

                using (var cmd = db.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "CALL curiosity.spGetUser(@license, @username, @discordId);";
                    cmd.Parameters.AddWithValue("@license", license);
                    cmd.Parameters.AddWithValue("@username", player.Name);
                    cmd.Parameters.AddWithValue("@discordId", discordId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (!reader.HasRows)
                            {
                                player.Drop($"Sorry {player.Name}, an error occurred while you were trying to connect to the server or update your characters information, please try to connect again. If the issue persists visit our Discord @ {CuriosityPlugin.DiscordUrl}");
                                throw new Exception($"SQL ERROR -> Failed to find information for {player.Name} [{player.Identifiers["license"]}]");
                            }

                            DataTable dataTable = new DataTable();
                            dataTable.Load(reader);

                            foreach (DataColumn dc in dataTable.Columns)
                            {
                                Logger.Info($"{dc.ColumnName}:{dataTable.Rows[0][dc.ColumnName]}");
                            }
                        }
                    }
                }
            }

            return user;
        }
    }
}
