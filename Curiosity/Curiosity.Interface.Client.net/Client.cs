using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net;
using System;
using System.Threading.Tasks;

namespace Curiosity.Interface.Client.net
{
    public class Client : BaseScript
    {
        public static int PedHandle { get { return Game.PlayerPed.Handle; } }
        public static int PlayerHandle { get { return Game.Player.Handle; } }
        public static PlayerList players;
        public static bool isSessionActive = false;
        public static bool clientSpawned = false;
        public static bool hideHud = false;

        private static Client _instance;

        public static Client GetInstance()
        {
            return _instance;
        }

        public Client()
        {
            _instance = this;
            players = Players;
            RegisterEventHandler("curiosity:Client:Player:SessionActivated", new Action(OnSessionActive));
            RegisterEventHandler("curiosity:Client:Player:HideHud", new Action<bool>(OnHideHud));
            RegisterEventHandler("playerSpawned", new Action(OnPlayerSpawned));

            ClassLoader.Init();

            Log.Info("Interface loaded\n");

        }

        async void OnSessionActive()
        {
            // BaseScript.TriggerServerEvent("curiosity:Server:Character:RoleCheck");
            await Delay(1000);
            isSessionActive = true;
        }

        void OnPlayerSpawned()
        {
            clientSpawned = true;
        }

        void OnHideHud(bool hudState)
        {
            hideHud = hudState;
        }

        /// <summary>
        /// Default/main tick function
        /// </summary>
        /// <returns></returns>
        public async Task OnTick()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Log.Error($"[CurrentVehicle] {ex.Message}");
            }

            await Task.FromResult(0);
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
                // Debug.WriteLine($"{action.Method.Name}");
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

        /// <summary>
        /// Registers an export, functions to be used by other resources. Registered exports still have to be defined in the __resource.lua file
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void RegisterExport(string name, Delegate action)
        {
            Exports.Add(name, action);
        }
    }
}
