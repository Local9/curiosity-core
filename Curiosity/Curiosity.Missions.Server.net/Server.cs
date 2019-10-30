using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Server.net.Helpers;
using System;
using System.Threading.Tasks;

namespace Curiosity.Missions.Server.net
{
    public class Server : BaseScript
    {
        private static Server _server;
        public static PlayerList players;
        public static bool isLive = false;

        public static Server GetInstance()
        {
            return _server;
        }

        public Server()
        {
            players = Players;
            isLive = API.GetConvar("server_live", "false") == "true";

            _server = this;

            // Environment
            Environment.MissionManager.Init();

            // EVENTS
            RegisterEventHandler("playerDropped", new Action<Player, string>(OnPlayerDropped));

            Log.Success("Leaving Curiosity Missions cter");
        }

        static void OnPlayerDropped([FromSource]CitizenFX.Core.Player player, string reason)
        {
            // Clean up and remove from any active missions
            Environment.MissionManager.OnPlayerDropped(player, reason);
        }

        public void RegisterEventHandler(string name, Delegate action)
        {
            EventHandlers[name] += action;
        }

        /// <summary>
        /// Registers a tick function
        /// </summary>
        /// <param name="action"></param>
        public void RegisterTickHandler(Func<Task> action)
        {
            try
            {
                Log.Verbose($"Server Tick -> Added {action.Method} Tick");
                Tick += action;
            }
            catch (Exception ex)
            {
                Log.Error($"RegisterTickHandler -> {ex.Message}");
            }
        }

        /// <summary>
        /// Deregisters a tick function
        /// </summary>
        /// <param name="action"></param>
        public void DeregisterTickHandler(Func<Task> action)
        {
            try
            {
                Log.Verbose($"Server Tick -> Removed {action.Method} Tick");
                Tick -= action;
            }
            catch (Exception ex)
            {
                Log.Error($"RegisterTickHandler -> {ex.Message}");
            }
        }
    }
}
