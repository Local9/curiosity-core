using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Threading.Tasks;

namespace Curiosity.Server.net
{
    public class CuriosityPlayer : BaseScript
    {
        const string LICENSE_IDENTIFIER = "license";
        bool isLive = false;

        Business.BusinessUser businessUser;

        public CuriosityPlayer()
        {
            // FiveM Events
            EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
            EventHandlers["playerConnecting"] += new Action<Player, string, dynamic, dynamic>(OnPlayerConnecting);
            EventHandlers["playerDropped"] += new Action<Player, string>(OnPlayerDropped);
            // Sends data to the client
            EventHandlers["curiosity:Server:Player:Setup"] += new Action<Player>(OnSetupPlayer);
            EventHandlers["curiosity:Server:Player:GetRole"] += new Action<Player>(GetUserRole);
            // Saves Data
            EventHandlers["curiosity:Server:Player:SaveLocation"] += new Action<Player, float, float, float>(OnSaveLocation);
            // Internal Events
            EventHandlers["curiosity:Server:Player:GetRoleId"] += new Action<int>(GetUserRoleId);
            EventHandlers["curiosity:Server:Player:GetUserId"] += new Action<Player>(GetUserId);

            isLive = API.GetConvar("server_live", "false") == "true";
        }

        private void OnResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            businessUser = Business.BusinessUser.GetInstance();

            WriteConsoleLine(string.Empty);
            WriteConsoleLine("-----------------------------------------------------------------");
            WriteConsoleLine("-> CURIOSITY PLAYER RESOURCE STARTED <---------------------------");
            WriteConsoleLine("-> IF A [SESSION ID] IS OVER 65K THEY WILL ERROR <---------------");
            WriteConsoleLine("-> IF THEY COMPLAIN ABOUT NOT GETTING EXPERIENCE, THIS IS WHY <--");
            WriteConsoleLine("-> END OF WARNINGS <---------------------------------------------");
            WriteConsoleLine("-----------------------------------------------------------------");
            Console.WriteLine();
        }

        void WriteConsoleLine(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine(message.PadRight(Console.WindowWidth));
            Console.ResetColor();
        }

        void OnPlayerConnecting([FromSource]Player player, string playerName, dynamic setKickReason, dynamic deferrals)
        {
            string license = player.Identifiers[LICENSE_IDENTIFIER];

            if (string.IsNullOrEmpty(license))
            {
                deferrals.done("License Not Found.");
            }

            // await SetupPlayerAsync(player);
        }

        async void OnPlayerDropped([FromSource]Player player, string reason)
        {
            if (Classes.SessionManager.PlayerList.ContainsKey(player.Handle))
            {
                Classes.SessionManager.PlayerList[player.Handle].Dropped(reason);
            }
            await Delay(0);
        }

        async void OnSetupPlayer([FromSource]Player player)
        {
            await SetupPlayerAsync(player);
        }

        async Task SetupPlayerAsync(Player player)
        {
            try
            {
                await Delay(3000);

                string license = player.Identifiers[LICENSE_IDENTIFIER];

                if (string.IsNullOrEmpty(license))
                {
                    throw new Exception("LICENSE MISSING");
                }

                Entity.User user = await businessUser.GetUserAsync(license);
                await Delay(0);
                Vector3 vector3 = await businessUser.GetUserLocationAsync(user.LocationId);
                await Delay(0);
                player.TriggerEvent("curiosity:Client:Player:Setup", user.UserId, user.RoleId, user.Role, vector3.X, vector3.Y, vector3.Z);
                await Delay(1000);

                Classes.Session session = new Classes.Session(player);
                session.UserID = user.UserId;
                session.Privilege = (Enums.Privilege)user.RoleId;
                session.LocationId = user.LocationId;
                session.IncreaseWallet(user.Wallet);
                session.IncreaseBankAccount(user.BankAccount);
                session.Activate();
                Debug.WriteLine($"session.Activate() -> {session}");
                await Delay(0);
                player.TriggerEvent("curiosity:Client:Rank:SetInitialXpLevels", user.WorldExperience, true, true);
                await Delay(0);
                player.TriggerEvent("curiosity:Client:Player:SessionCreated", user.UserId);
                await Delay(0);
                player.TriggerEvent("curiosity:Player:Bank:UpdateWallet", session.Wallet);
                await Delay(0);
                player.TriggerEvent("curiosity:Player:Bank:UpdateBank", session.BankAccount);
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

                string license = player.Identifiers[LICENSE_IDENTIFIER];

                if (string.IsNullOrEmpty(license))
                {
                    throw new Exception("LICENSE MISSING");
                }

                await businessUser.SavePlayerLocationAsync(license, x, y, z);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnSaveLocation -> {ex.Message}");
            }
        }

        async void GetUserRole([FromSource]Player player)
        {
            await Delay(0);

            string license = player.Identifiers[LICENSE_IDENTIFIER];

            if (string.IsNullOrEmpty(license))
            {
                throw new Exception("LICENSE MISSING");
            }

            Entity.User user = await businessUser.GetUserAsync(license);

            player.TriggerEvent("curiosity:Client:Player:Role", user.Role);
        }

        async void GetUserRoleId(int playerHandle)
        {
            Player player = new PlayerList()[playerHandle];
            string license = player.Identifiers[LICENSE_IDENTIFIER];
            Entity.User user = await businessUser.GetUserAsync(license);
            TriggerEvent("curiosity:Server:Player:RoleId", user.RoleId);
        }

        async void GetUserId([FromSource]Player player)
        {
            if (!Classes.SessionManager.PlayerList.ContainsKey(player.Handle))
            {
                player.TriggerEvent("curiosity:Client:Player:UserId", null);
                return;
            }
            long userId = Classes.SessionManager.GetUserId($"{player.Handle}");
            player.TriggerEvent("curiosity:Client:Player:UserId", userId);
            await Delay(0);
        }
    }
}
