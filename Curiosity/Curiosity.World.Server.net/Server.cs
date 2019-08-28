using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Threading.Tasks;
using Curiosity.Shared.Server.net.Helpers;

namespace Curiosity.World.Server.net
{
    public class Server : BaseScript
    {
        private static Server _server;

        public static Server GetInstance()
        {
            return _server;
        }

        public Server()
        {
            Log.Success("Entering Curiosity Weather cter");

            _server = this;

            Classes.Environment.WeatherSystems.Init();
            Classes.Environment.WorldTimeCycle.Init();

            Log.Success("Exiting Curiosity Weather cter");
        }

        public void RegisterEventHandler(string name, Delegate action)
        {
            EventHandlers[name] += action;
        }
        
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
