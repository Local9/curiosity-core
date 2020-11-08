using CitizenFX.Core;
using Curiosity.MissionManager.Server.Diagnostics;
using Curiosity.Systems.Library.Models;
using System;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Server.Database.Store
{
    class UserDatabase
    {
        public static async Task<CuriosityUser> Get(Player player, ulong discordId)
        {
            try
            {
                Logger.Debug($"User: {player.Name}, DiscordId: {discordId}");

                // Data from Curiosity-Server

                CuriosityUser curiosityUser = new CuriosityUser();

                return curiosityUser;
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
                return null;
            }
        }
    }
}
