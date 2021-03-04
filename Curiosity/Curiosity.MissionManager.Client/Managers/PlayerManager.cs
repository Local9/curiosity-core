using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.Entity;
using Curiosity.Global.Shared.Enums;
using Curiosity.MissionManager.Client.Events;
using Curiosity.MissionManager.Client.Managers;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.EventWrapperLegacy;
using System;

namespace Curiosity.MissionManager.Client.Manager
{
    public class PlayerManager : Manager<PlayerManager>
    {
        const string PERSONAL_VEHICLE_KEY = "PERSONAL_VEHICLE_ID";

        public PlayerInformationModel playerInfo = new PlayerInformationModel();
        public Privilege privilege;
        public Vehicle PersonalVehicle;

        public PlayerManager()
        {
            Instance.EventRegistry[LegacyEvents.Native.Client.PlayerSpawned] += new Action<dynamic>(OnPlayerSpawned);
            Instance.EventRegistry[LegacyEvents.Native.Client.OnClientResourceStart.Path] += LegacyEvents.Native.Client.OnClientResourceStart.Action += OnClientResourceStart;
        }

        public void SetVehicle(int vehicleId)
        {
            if (PersonalVehicle == null)
            {
                API.SetResourceKvpInt(PERSONAL_VEHICLE_KEY, vehicleId);
                PersonalVehicle = new Vehicle(vehicleId);
                Decorators.Set(vehicleId, Decorators.PLAYER_VEHICLE, true);
                Decorators.Set(vehicleId, Decorators.PLAYER_OWNER, Game.Player.ServerId);
            }
            else if (PersonalVehicle.Handle != vehicleId)
            {
                API.SetResourceKvpInt(PERSONAL_VEHICLE_KEY, vehicleId);
                PersonalVehicle = new Vehicle(vehicleId);
                Decorators.Set(vehicleId, Decorators.PLAYER_VEHICLE, true);
                Decorators.Set(vehicleId, Decorators.PLAYER_OWNER, Game.Player.ServerId);
            }
            else
            {
                int vehId = API.GetResourceKvpInt(PERSONAL_VEHICLE_KEY);
                if (vehId > 0)
                {
                    Vehicle v = new Vehicle(vehId);
                    if (v == null) return;
                    if (!v.Exists()) return;
                    if (v.IsDead) return;
                    PersonalVehicle = v;
                    Decorators.Set(vehId, Decorators.PLAYER_VEHICLE, true);
                    Decorators.Set(vehId, Decorators.PLAYER_OWNER, Game.Player.ServerId);
                }
            }

            if (PersonalVehicle != null)
            {
                API.SetVehicleCanBeLockedOn(PersonalVehicle.Handle, false, false);
                API.SetVehicleCanBeUsedByFleeingPeds(PersonalVehicle.Handle, false);
                EventSystem.GetModule().Send("user:personal:vehicle", PersonalVehicle.NetworkId);
            }
        }

        private void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            int vehicleId = API.GetResourceKvpInt(PERSONAL_VEHICLE_KEY);

            if (vehicleId > 0)
            {
                Vehicle kvpVehicle = new Vehicle(vehicleId);
                if (kvpVehicle.Exists())
                {
                    if (!kvpVehicle.IsDead)
                    {
                        PersonalVehicle = kvpVehicle;
                        Decorators.Set(vehicleId, Decorators.PLAYER_VEHICLE, true);
                        Decorators.Set(vehicleId, Decorators.PLAYER_OWNER, Game.Player.ServerId);
                    }
                }
            }

        }

        private void OnPlayerSpawned(dynamic spawnObject)
        {
        }
    }
}
