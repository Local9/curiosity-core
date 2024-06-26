﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net;
using System;
using System.Threading.Tasks;
using GlobalEntity = Curiosity.Global.Shared.Entity;

namespace Curiosity.Menus.Client.net
{
    public class Client : BaseScript
    {
        private const string PERSONAL_VEHICLE_ID = "PERSONAL_VEHICLE_ID";
        private static Client _instance;
        public static PlayerList players;
        public static GlobalEntity.User User;
        public static Random Random = new Random();

        public static int PedHandle { get { return Game.PlayerPed.Handle; } }
        public static int PlayerHandle { get { return Game.Player.Handle; } }

        public static bool hasPlayerSpawned = false;

        public static Vehicle CurrentVehicle = null;

        public static Client GetInstance()
        {
            return _instance;
        }

        public Client()
        {
            _instance = this;
            CurrentVehicle = null;

            players = Players;

            ClassLoader.Init();

            RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));

            RegisterEventHandler("playerSpawned", new Action<dynamic>(OnPlayerSpawned));
            RegisterEventHandler("curiosity:Client:Player:Setup", new Action<string>(OnPlayerSetup));
            RegisterEventHandler("curiosity:Player:Menu:VehicleId", new Action<int>(OnVehicleId));

            Log.Info("Curiosity.Menus.Client.net loaded\n");
        }

        private void OnVehicleId(int vehicleHandle)
        {
            if (CurrentVehicle == null)
            {
                API.SetResourceKvpInt(PERSONAL_VEHICLE_ID, vehicleHandle);
                CurrentVehicle = new Vehicle(vehicleHandle);
            }
            else if (Client.CurrentVehicle.Handle != vehicleHandle)
            {
                API.SetResourceKvpInt(PERSONAL_VEHICLE_ID, vehicleHandle);
                CurrentVehicle = new Vehicle(vehicleHandle);
            }
        }

        static void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;
            Client.TriggerEvent("curiosity:Client:Player:Information");

            int vehicleHandle = API.GetResourceKvpInt(PERSONAL_VEHICLE_ID);
            if (vehicleHandle > 0)
            {
                if (API.DoesEntityExist(vehicleHandle))
                {
                    if (!API.IsEntityDead(vehicleHandle))
                    {
                        CurrentVehicle = new Vehicle(vehicleHandle);
                    }
                }
            }
        }

        async void OnPlayerSetup(string jsonUser)
        {
            User = Newtonsoft.Json.JsonConvert.DeserializeObject<GlobalEntity.User>(jsonUser);

            if (User.Skin == null)
            {
                User.Skin = new GlobalEntity.PlayerCharacter();
            }
        }

        void OnPlayerSpawned(dynamic spawnedPlayer)
        {
            hasPlayerSpawned = true;
            Client.TriggerEvent("curiosity:Client:Player:Information");
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
