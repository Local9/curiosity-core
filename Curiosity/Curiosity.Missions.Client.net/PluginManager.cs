using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Missions.Client.Classes.PlayerClient;
using Curiosity.Missions.Client.Managers;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Enums.Patrol;
using System;
using System.Threading.Tasks;

namespace Curiosity.Missions.Client
{
    public class PluginManager : BaseScript
    {
        internal static PluginManager Instance { get; private set; }

        public static PlayerList players;

        public static bool IsBirthday;
        public static bool IsPlayerSpawned;

        private static Vehicle _currentVehicle;
        public static SpeechType speechType;
        public static bool DeveloperNpcUiEnabled, DeveloperVehUiEnabled;


        public const string MOVEMENT_ANIMATION_SET_DRUNK = "MOVE_M@DRUNK@VERYDRUNK";
        // DECOR HANDLES
        public const string DECOR_NPC_CURRENT_VEHICLE = "curiosity::npc::vehicle";
        public const string DECOR_NPC_WAS_RELEASED = "curiosity::npc::released";
        public const string DECOR_NPC_ACTIVE_TRAFFIC_STOP = "curiosity::npc::trafficStopActive";
        public const string DECOR_NPC_RAN_FROM_POLICE = "curiosity::npc::runner";

        public const string DECOR_VEHICLE_HAS_BEEN_TRAFFIC_STOPPED = "curiosity::vehicle::trafficStop";
        public const string DECOR_VEHICLE_IGNORE = "curiosity::vehicle::ignore";
        public const string DECOR_VEHICLE_DETECTED_BY = "curiosity::vehicle::detected";
        public const string DECOR_VEHICLE_SPEEDING = "curiosity::police::speeding";
        public const string DECOR_VEHICLE_STOLEN = "curiosity::police::ped::stolenCar";

        public const string DECOR_PLAYER_VEHICLE = "Player_Vehicle";
        public const string DECOR_TRAFFIC_STOP_VEHICLE_HANDLE = "curiosity::traffic_stop::vehicle_handle";

        public const string DECOR_NPC_CAN_BE_ARRESTED = "curiosity::police::ped::canBeArrested";
        public const string DECOR_NPC_HANDCUFF = "curiosity::police::ped::handcuff";
        public const string DECOR_NPC_ARRESTED = "curiosity::police::ped::arrested";
        public const string DECOR_NPC_ITEM_ILLEGAL = "curiosity::police::ped::illegalItems";
        public const string DECOR_NPC_ITEM_STOLEN = "curiosity::police::ped::stolenItems";
        public const string DECOR_NPC_DRUG_ALCOHOL = "curiosity::police::ped::alcohol";
        public const string DECOR_NPC_DRUG_CANNABIS = "curiosity::police::ped::cannabis";
        public const string DECOR_NPC_DRUG_COCAINE = "curiosity::police::ped::cocaine";
        public const string DECOR_NPC_VEHICLE_HANDLE = "curiosity::police::ped::vehicleHandle";
        public const string DECOR_INTERACTION_CAN_BE_SEARCHED = "curiosity::police::ped::canSearch";
        public const string DECOR_INTERACTION_HAS_BEEN_SEARCHED = "curiosity::police::ped::hasBeenSearched";
        public const string DECOR_INTERACTION_PROVIDED_ID = "curiosity::police::ped::providedId";
        public const string DECOR_INTERACTION_RAN_ID = "curiosity::police::ped::idRan";
        public const string DECOR_INTERACTION_LOST_ID = "curiosity::police::ped::lostId";
        public const string DECOR_INTERACTION_GRABBED = "curiosity::police::ped::grabbed";
        public const string DECOR_INTERACTION_CORONER_CALLED = "curiosity::police::ped::coronerCalled";
        public const string DECOR_INTERACTION_WANTED = "curiosity::police::ped::wanted";

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

        public PluginManager()
        {
            Instance = this;

            players = Players;

            CurrentVehicle = null;

            RegisterEventHandler("curiosity:Player:Menu:VehicleId", new Action<int>(OnVehicleId));
            RegisterEventHandler("curiosity:Player:Mission:SpeechType", new Action<int>(OnSpeechType));
            RegisterEventHandler("curiosity:Player:Mission:ShowDeveloperNpcUI", new Action<bool>(OnShowDeveloperNpcUi));
            RegisterEventHandler("curiosity:Player:Mission:ShowDeveloperVehUI", new Action<bool>(OnShowDeveloperVehUi));

            RegisterEventHandler("curiosity:client:special", new Action<bool>(OnSpecialDay));
            RegisterEventHandler("playerSpawned", new Action<dynamic>(OnPlayerSpawned));

            API.DecorRegister(DECOR_NPC_CURRENT_VEHICLE, 3); // int
            API.DecorRegister(DECOR_TRAFFIC_STOP_VEHICLE_HANDLE, 3);
            API.DecorRegister(DECOR_VEHICLE_DETECTED_BY, 3);
            API.DecorRegister(DECOR_NPC_ARRESTED, 2); // bool
            API.DecorRegister(DECOR_NPC_WAS_RELEASED, 2);
            API.DecorRegister(DECOR_NPC_ACTIVE_TRAFFIC_STOP, 2);
            API.DecorRegister(DECOR_VEHICLE_HAS_BEEN_TRAFFIC_STOPPED, 2);
            API.DecorRegister(DECOR_VEHICLE_IGNORE, 2);
            API.DecorRegister(DECOR_PLAYER_VEHICLE, 2);
            API.DecorRegister(DECOR_NPC_RAN_FROM_POLICE, 2);
            API.DecorRegister(DECOR_VEHICLE_SPEEDING, 2);

            ClassLoader.Init();

            Log.Info("Curiosity.Missions.Client loaded\n");
        }

        private void OnPlayerSpawned(dynamic obj)
        {
            IsPlayerSpawned = true;
        }

        [Tick]
        private async Task OnSpecialDayTick()
        {
            if (IsPlayerSpawned)
            {

                long gameTimer = API.GetGameTimer();
                while ((API.GetGameTimer() - gameTimer) < 30000)
                {
                    await PluginManager.Delay(100);
                }

                TriggerServerEvent("curiosity:server:special");
            }
        }

        private void OnSpecialDay(bool isBirthday)
        {
            if (!isBirthday && IsBirthday)
            {
                Screen.ShowNotification("The event has now ended, thank you all!", true);
            }

            if (!IsBirthday && isBirthday)
            {
                Screen.ShowNotification("~w~Its ~g~127.0.0.1~w~ Birthday!!!", true);
                Screen.ShowNotification("Enjoy increased Police XP from callouts and traffic stops!");
            }

            IsBirthday = isBirthday;
        }

        static void OnSpeechType(int speech)
        {
            speechType = (SpeechType)speech;
        }

        static void OnShowDeveloperNpcUi(bool state)
        {
            DeveloperNpcUiEnabled = state;
        }

        static void OnShowDeveloperVehUi(bool state)
        {
            DeveloperVehUiEnabled = state;
        }

        public static void OnVehicleId(int vehicleId)
        {
            if (PluginManager.CurrentVehicle == null)
            {
                API.SetResourceKvpInt(PERSONAL_VEHICLE_KEY, vehicleId);
                PluginManager.CurrentVehicle = new Vehicle(vehicleId);
                API.DecorSetBool(PluginManager.CurrentVehicle.Handle, DECOR_PLAYER_VEHICLE, true);
            }
            else if (PluginManager.CurrentVehicle.Handle != vehicleId)
            {
                API.SetResourceKvpInt(PERSONAL_VEHICLE_KEY, vehicleId);
                PluginManager.CurrentVehicle = new Vehicle(vehicleId);
                API.DecorSetBool(PluginManager.CurrentVehicle.Handle, DECOR_PLAYER_VEHICLE, true);
            }

            if (CurrentVehicle == null)
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    CurrentVehicle = Game.PlayerPed.CurrentVehicle;
                    API.DecorSetBool(PluginManager.CurrentVehicle.Handle, DECOR_PLAYER_VEHICLE, true);
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
                Log.Error(ex, "RegisterEventHandler");
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
                Log.Error(ex, "RegisterNuiEventHandler");
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
                if (PlayerManager.IsDeveloper)
                    Debug.WriteLine($"REGISTERED: {action.Method.Name}");

                Tick += action;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "RegisterTickHandler");
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
                if (PlayerManager.IsDeveloper)
                    Debug.WriteLine($"DEREGISTERED: {action.Method.Name}");

                Tick -= action;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DeregisterTickHandler");
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
