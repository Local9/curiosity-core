using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Threading.Tasks;

namespace Curiosity.Server.Net
{
    public class CuriosityPlayer : BaseScript
    {
        const string STEAM_IDENTIFIER = "steam";
        bool isLive = false;

        Business.BusinessUser businessUser;

        public CuriosityPlayer()
        {
            EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);

            EventHandlers["curiosity:Server:Player:Setup"] += new Action<Player>(OnPlayerSetup);

            isLive = API.GetConvar("server_live", "false") == "true";

            // Tick += TestQuery;
        }

        async Task TestQuery()
        {
            while (!isLive)
            {
                await Delay(1000);
                await businessUser.TestQueryAsync();
            }
            await Delay(60000);
        }

        private void OnResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            businessUser = Business.BusinessUser.GetInstance();
        }

        async void OnPlayerSetup([FromSource]Player player)
        {
            try
            {
                await Delay(10);

                string steamId = player.Identifiers[STEAM_IDENTIFIER];

                if (string.IsNullOrEmpty(steamId))
                {
                    throw new Exception("STEAMID MISSING");
                }

                long userId = await businessUser.GetUserIdAsync(steamId);

                player.TriggerEvent("curiosity:Client:Player:Setup", userId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnPlayerSetup -> {ex.Message}");
            }
        }
    }
}
