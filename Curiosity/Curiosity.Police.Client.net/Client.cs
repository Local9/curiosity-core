using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net;
using System;
using System.Threading.Tasks;

namespace Curiosity.Police.Client.net
{
    public class Client : BaseScript
    {
        private static Client _instance;
        public static PlayerList players;

        public static Vehicle CurrentVehicle;
        private const string PERSONAL_VEHICLE_KEY = "PERSONAL_VEHICLE_ID";

        public static Client GetInstance()
        {
            return _instance;
        }

        public Client()
        {
            _instance = this;

            CurrentVehicle = null;

            RegisterEventHandler("curiosity:Player:Menu:VehicleId", new Action<int>(OnVehicleId));

            RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));

            players = Players;

            ClassLoader.Init();

            Log.Info("Curiosity.Mobile.Client.net loaded\n");
        }

        static void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;
        }

        public static void OnVehicleId(int vehicleId)
        {
            if (CurrentVehicle == null)
            {
                API.SetResourceKvpInt(PERSONAL_VEHICLE_KEY, vehicleId);
                CurrentVehicle = new Vehicle(vehicleId);
            }
            else if (Client.CurrentVehicle.Handle != vehicleId)
            {
                API.SetResourceKvpInt(PERSONAL_VEHICLE_KEY, vehicleId);
                CurrentVehicle = new Vehicle(vehicleId);
            }
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
                if (Classes.Player.PlayerInformation.IsDeveloper())
                {
                    Log.Info($"Tick Registered -> {action.Method.Name}");
                }
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
                if (Classes.Player.PlayerInformation.IsDeveloper())
                {
                    Log.Info($"Tick Deregistered -> {action.Method.Name}");
                }
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
