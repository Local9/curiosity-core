using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net;
using System;
using System.Threading.Tasks;
using Curiosity.Shared.Client.net.Enums.Patrol;

namespace Curiosity.Missions.Client.net
{
    public class Client : BaseScript
    {
        private static Client _instance;
        public static PlayerList players;
        public static Random Random;

        private static Vehicle _currentVehicle;
        public static SpeechType speechType;
        public static bool DeveloperUiEnabled;

        public static Vehicle CurrentVehicle
        {
            get
            {
                if (_currentVehicle == null)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        if (Game.PlayerPed.CurrentVehicle.Driver == Game.PlayerPed)
                            _currentVehicle = Game.PlayerPed.CurrentVehicle;
                    }
                }
                return _currentVehicle;
            }
            set
            {
                _currentVehicle = value;
            }
        }

        private const string PERSONAL_VEHICLE_KEY = "PERSONAL_VEHICLE_ID";

        public static Client GetInstance()
        {
            if (Random == null)
            {
                Random = new Random();
            }

            return _instance;
        }

        public Client()
        {
            _instance = this;

            players = Players;
            CurrentVehicle = null;

            RegisterEventHandler("curiosity:Player:Menu:VehicleId", new Action<int>(OnVehicleId));
            RegisterEventHandler("curiosity:Player:Mission:SpeechType", new Action<int>(OnSpeechType));
            RegisterEventHandler("curiosity:Player:Mission:ShowDeveloperUI", new Action<bool>(OnShowDeveloperUi));

            ClassLoader.Init();

            Log.Info("Curiosity.Missions.Client.net loaded\n");
        }

        static void OnSpeechType(int speech)
        {
            speechType = (SpeechType)speech;
        }

        static void OnShowDeveloperUi(bool state)
        {
            DeveloperUiEnabled = state;
        }

        public static void OnVehicleId(int vehicleId)
        {
            if (Client.CurrentVehicle == null)
            {
                API.SetResourceKvpInt(PERSONAL_VEHICLE_KEY, vehicleId);
                Client.CurrentVehicle = new Vehicle(vehicleId);
            }
            else if (Client.CurrentVehicle.Handle != vehicleId)
            {
                API.SetResourceKvpInt(PERSONAL_VEHICLE_KEY, vehicleId);
                Client.CurrentVehicle = new Vehicle(vehicleId);
            }

            if (CurrentVehicle == null)
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    CurrentVehicle = Game.PlayerPed.CurrentVehicle;
                }
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
                if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                    Debug.WriteLine($"REGISTERED: {action.Method.Name}");

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
                if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                    Debug.WriteLine($"DEREGISTERED: {action.Method.Name}");

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
