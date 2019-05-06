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
            EventHandlers["playerConnecting"] += new Action<Player, string, dynamic, dynamic>(OnPlayerConnecting);

            EventHandlers["curiosity:Server:Player:Setup"] += new Action<Player>(OnSetupPlayer);
            EventHandlers["curiosity:Server:Player:SaveLocation"] += new Action<Player, float, float, float>(OnSaveLocation);

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

        async void OnPlayerConnecting([FromSource]Player player, string playerName, dynamic setKickReason, dynamic deferrals)
        {
            await SetupPlayerAsync(player);
        }

        async void OnSetupPlayer([FromSource]Player player)
        {
            await SetupPlayerAsync(player);
        }

        async Task SetupPlayerAsync(Player player)
        {
            try
            {
                await Delay(0);

                string steamId = player.Identifiers[STEAM_IDENTIFIER];

                if (string.IsNullOrEmpty(steamId))
                {
                    throw new Exception("STEAMID MISSING");
                }

                Entity.User user = await businessUser.GetUserIdAsync(steamId);
                await Delay(0);
                Vector3 vector3 = await businessUser.GetUserLocationAsync(user.LocationId);

                player.TriggerEvent("curiosity:Client:Player:Setup", user.UserId, vector3.X, vector3.Y, vector3.Z);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnPlayerSetup -> {ex.Message}");
            }
        }

        async void OnSaveLocation([FromSource]Player player, float x, float y, float z)
        {
            try
            {
                await Delay(0);

                string steamId = player.Identifiers[STEAM_IDENTIFIER];

                if (string.IsNullOrEmpty(steamId))
                {
                    throw new Exception("STEAMID MISSING");
                }

                await businessUser.SavePlayerLocationAsync(steamId, x, y, z);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnSaveLocation -> {ex.Message}");
            }
        }
    }
}
