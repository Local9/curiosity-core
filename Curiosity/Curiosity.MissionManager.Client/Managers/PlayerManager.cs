using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.Entity;
using Curiosity.Global.Shared.Enums;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.EventWrapperLegacy;
using System;

namespace Curiosity.MissionManager.Client.Manager
{
    class PlayerManager : BaseScript
    {
        const string PERSONAL_VEHICLE_KEY = "PERSONAL_VEHICLE_ID";

        internal static ExportDictionary exportDictionary;

        internal static PlayerInformationModel playerInfo = new PlayerInformationModel();
        internal static Privilege privilege;
        internal static Vehicle PersonalVehicle;
        internal static Vehicle PersonalTrailer;

        public PlayerManager()
        {
            EventHandlers[LegacyEvents.Client.CurrentVehicle] += new Action<int>(OnVehicleId);

            EventHandlers[LegacyEvents.Native.Client.PlayerSpawned] += new Action<dynamic>(OnPlayerSpawned);
            EventHandlers[LegacyEvents.Native.Client.OnClientResourceStart.Path] += LegacyEvents.Native.Client.OnClientResourceStart.Action += OnClientResourceStart;
        }

        private static void OnVehicleId(int vehicleId)
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
