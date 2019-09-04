using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net;

namespace Curiosity.World.Client.net
{
    public class Client : BaseScript
    {
        public static PlayerList players;
        public static int PedHandle { get { return Game.PlayerPed.Handle; } }

        private static Client _instance;
        static public RelationshipGroup PlayerRelationshipGroup;

        public static Client GetInstance()
        {
            return _instance;
        }

        public Client()
        {
            _instance = this;
            ClassLoader.Init();

            RegisterEventHandler("playerSpawned", new Action<dynamic>(OnPlayerSpawned));
            RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));
        }

        static void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;
            PlayerRelationshipGroup = CitizenFX.Core.World.AddRelationshipGroup("PLAYER");
        }

        static void OnPlayerSpawned(dynamic spawndata)
        {
            PlayerRelationshipGroup = CitizenFX.Core.World.AddRelationshipGroup("PLAYER");
        }

        /// <summary>
        /// Registers a network event
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void RegisterEventHandler(string name, Delegate action)
        {
            try
            {
                EventHandlers[name] += action;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Registers a NUI/CEF event
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void RegisterNuiEventHandler(string name, Delegate action)
        {
            try
            {
                Function.Call(Hash.REGISTER_NUI_CALLBACK_TYPE, name);
                RegisterEventHandler(string.Concat("__cfx_nui:", name), action);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Registers a tick function
        /// </summary>
        /// <param name="action"></param>
        public void RegisterTickHandler(Func<Task> action)
        {
            try
            {
                Tick += action;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Removes a tick function from the registry
        /// </summary>
        /// <param name="action"></param>
        public void DeregisterTickHandler(Func<Task> action)
        {
            try
            {
                Tick -= action;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
    }
}
