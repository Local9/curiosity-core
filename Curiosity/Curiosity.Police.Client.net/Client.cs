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

            // RegisterTickHandler(Spin);

            ClassLoader.Init();

            Log.Info("Curiosity.Mobile.Client.net loaded\n");
        }

        static int platform;
        static float heading;

        static Vehicle Vehicle;

        private async static Task Spin()
        {
            if (Game.PlayerPed.Position.DistanceToSquared(new Vector3(1100f, 220f, -50f)) > 100f)
            {
                if (Vehicle != null)
                {
                    if (Vehicle.Exists())
                        Vehicle.Delete();
                }
            }

            if (!API.DoesEntityExist(platform))
            {
                platform = API.GetClosestObjectOfType(1100f, 220f, -50f, 1f, (uint)API.GetHashKey("vw_prop_vw_casino_podium_01a"), false, false, false);
            }
            else
            {
                if (Vehicle == null)
                {
                    CreateVehicle();
                }

                heading = (heading + (4f * API.Timestep()));

                if (heading >= 360)
                {
                    heading = heading - 360;
                }

                API.SetEntityHeading(platform, heading);
            }
        }

        private static async void CreateVehicle()
        {
            if (Vehicle != null)
            {
                if (Vehicle.Exists())
                    Vehicle.Delete();
            }

            Model model = API.GetHashKey("formula");
            model.Request(10000);
            while (!model.IsLoaded)
            {
                await Client.Delay(0);
            }

            if (model.IsLoaded)
            {

                Vehicle = await World.CreateVehicle(model, new Vector3(1100f, 220f, -50f), heading);
                Vehicle.IsPositionFrozen = true;
                Vehicle.IsCollisionEnabled = false;
                Vehicle.IsInvincible = true;
                API.N_0xab04325045427aae(Vehicle.Handle, false);
                API.N_0xdbc631f109350b8c(Vehicle.Handle, true);
                API.N_0x2311dd7159f00582(Vehicle.Handle, true);

                API.AttachEntityToEntity(Vehicle.Handle, platform, -1, 0f, 0f, 0.52f, 0f, 0f, 0f, false, false, false, false, 2, true);

                model.MarkAsNoLongerNeeded();
            }
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

        public void TriggerEventForAll(string eventName, params object[] args)
        {
            Debug.WriteLine(eventName);
            TriggerServerEvent("curiosity:Server:Event:ForAllPlayers", eventName, args);
        }
    }
}
