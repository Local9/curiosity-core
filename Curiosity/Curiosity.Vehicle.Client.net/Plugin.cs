using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Vehicles.Client.net
{
    public class Plugin : BaseScript
    {
        private static Plugin _instance;
        public static PlayerList players;
        public static CitizenFX.Core.Vehicle CurrentVehicle;

        public static Random random = new Random();

        public static RelationshipGroup PlayerRelationshipGroup;
        public static RelationshipGroup MechanicRelationshipGroup;

        public static string STAFF_LICENSE_PLATE = "LV0STAFF";
        public static string HSTAFF_LICENSE_PLATE = "LV0HSTAF";
        public static string DEV_LICENSE_PLATE = "LIFEVDEV";
        public const string TROUBLE_LICENSE_PLATE = "TROUBLES";

        public const string DECOR_VEHICLE_SAFEZONE_TIME = "c::vehicle::safezone::time";
        public const string DECOR_VEHICLE_SAFEZONE_INSIDE = "c::vehicle::safezone::inside";

        // decor
        public const string PLAYER_VEHICLE = "Player_Vehicle";

        public static Plugin GetInstance()
        {
            return _instance;
        }

        public Plugin()
        {
            _instance = this;

            players = Players;

            ClassLoader.Init();

            PlayerRelationshipGroup = World.AddRelationshipGroup("PLAYER");
            MechanicRelationshipGroup = World.AddRelationshipGroup("MECH");

            RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));
            RegisterEventHandler("onClientResourceStop", new Action<string>(OnClientResourceStop));

            Log.Info("Curiosity.Vehicles.Client.net loaded\n");


            // Register DECOR
            DecorRegister(PLAYER_VEHICLE, 3);
        }

        static void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            Plugin.TriggerEvent("curiosity:Client:Player:Information");

            Game.PlayerPed.RelationshipGroup = PlayerRelationshipGroup;

            if (Game.PlayerPed.IsInVehicle())
            {
                if (Game.PlayerPed.CurrentVehicle.Driver == Game.PlayerPed)
                {
                    CurrentVehicle = Game.PlayerPed.CurrentVehicle;
                    float fuel = API.GetResourceKvpFloat("VR_FUEL");
                    API.DecorSetFloat(CurrentVehicle.Handle, "Vehicle.Fuel", fuel);
                    CurrentVehicle.IsEngineRunning = true;
                }
            }
        }

        static void OnClientResourceStop(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            if (Game.PlayerPed.IsInVehicle())
            {
                if (Game.PlayerPed.CurrentVehicle.Driver == Game.PlayerPed)
                {
                    float fuel = API.DecorGetFloat(Plugin.CurrentVehicle.Handle, "Vehicle.Fuel");
                    API.SetResourceKvpFloat("VR_FUEL", fuel);
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
