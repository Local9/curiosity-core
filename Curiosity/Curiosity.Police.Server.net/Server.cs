using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Threading.Tasks;
using Curiosity.Shared.Server.net.Helpers;

namespace Curiosity.Police.Server.net
{
    public class Server : BaseScript
    {
        private static Server _server;
        public static PlayerList players;

        public static Server GetInstance()
        {
            return _server;
        }

        public Server()
        {
            Log.Success("Entering Curiosity Police cter");

            players = Players;

            _server = this;

            

            Log.Success("Leaving Curiosity Police cter");
        }

        private void OnResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            Log.Success("-----------------------------------------------------------------");
            Log.Success("-> CURIOSITY POLICE RESOURCE STARTING UP <-----------------------");
            Log.Success("-----------------------------------------------------------------");
        }

        static void OnPlayerConnecting([FromSource]CitizenFX.Core.Player player, string playerName, dynamic setKickReason, dynamic deferrals)
        {
            
        }

        static void OnPlayerDropped([FromSource]CitizenFX.Core.Player player, string reason)
        {
            
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
