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
            // FiveM Events
            EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
            EventHandlers["playerConnecting"] += new Action<Player, string, dynamic, dynamic>(OnPlayerConnecting);
            // Sends data to the client
            EventHandlers["curiosity:Server:Player:Setup"] += new Action<Player>(OnSetupPlayer);
            EventHandlers["curiosity:Server:Player:GetRole"] += new Action<Player>(GetUserRole);
            // Saves Data
            EventHandlers["curiosity:Server:Player:SaveLocation"] += new Action<Player, float, float, float>(OnSaveLocation);
            // Internal Events
            EventHandlers["curiosity:Server:Player:GetRoleId"] += new Action<int>(GetUserRoleId);

            isLive = API.GetConvar("server_live", "false") == "true";
        }

        private void OnResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            businessUser = Business.BusinessUser.GetInstance();
        }

        async void OnPlayerConnecting([FromSource]Player player, string playerName, dynamic setKickReason, dynamic deferrals)
        {
            string steamId = player.Identifiers[STEAM_IDENTIFIER];

            if (string.IsNullOrEmpty(steamId))
            {
                deferrals.done("SteamID Not Found. Please have Steam running to connect.");
            }

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

                Entity.User user = await businessUser.GetUserAsync(steamId);
                await Delay(0);
                Vector3 vector3 = await businessUser.GetUserLocationAsync(user.LocationId);
                await Delay(0);
                player.TriggerEvent("curiosity:Client:Player:Setup", user.UserId, user.RoleId, user.Role, vector3.X, vector3.Y, vector3.Z);
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

        async void GetUserRole([FromSource]Player player)
        {
            await Delay(0);

            string steamId = player.Identifiers[STEAM_IDENTIFIER];

            if (string.IsNullOrEmpty(steamId))
            {
                throw new Exception("STEAMID MISSING");
            }

            Entity.User user = await businessUser.GetUserAsync(steamId);

            player.TriggerEvent("curiosity:Client:Player:Role", user.Role);
        }

        async void GetUserRoleId(int playerHandle)
        {
            Player player = new PlayerList()[playerHandle];
            string steamId = player.Identifiers[STEAM_IDENTIFIER];
            Entity.User user = await businessUser.GetUserAsync(steamId);
            TriggerEvent("curiosity:Server:Player:RoleId", user.RoleId);
        }
    }
}
