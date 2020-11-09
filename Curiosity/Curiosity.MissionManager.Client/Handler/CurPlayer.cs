﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.Entity;
using Curiosity.Global.Shared.Enums;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.EventWrapperLegacy;
using System;
using static Curiosity.Systems.Library.EventWrapperLegacy.LegacyEvents;

namespace Curiosity.MissionManager.Client.Handler
{
    class CurPlayer : BaseScript
    {
        const string PERSONAL_VEHICLE_KEY = "PERSONAL_VEHICLE_ID";

        internal static ExportDictionary exportDictionary;

        internal static PlayerInformationModel playerInfo = new PlayerInformationModel();
        internal static Privilege privilege;
        internal static PatrolZone PatrolZone = PatrolZone.City;
        internal static Vehicle PersonalVehicle;
        internal static Vehicle PersonalTrailer;

        internal static bool IsOnDuty;
        internal static bool IsOfficer;

        public CurPlayer()
        {
            EventHandlers[LegacyEvents.Client.PolicePatrolZone] += new Action<int>(OnPlayerPatrolZone);
            EventHandlers[LegacyEvents.Client.PoliceDutyEvent] += new Action<bool, bool, string>(OnPoliceDuty);
            EventHandlers[LegacyEvents.Client.CurrentVehicle] += new Action<int>(OnVehicleId);

            EventHandlers[Native.Client.PlayerSpawned] += new Action<dynamic>(OnPlayerSpawned);
            EventHandlers[Native.Client.OnClientResourceStart.Path] += Native.Client.OnClientResourceStart.Action += OnClientResourceStart;
        }

        private static void OnVehicleId(int vehicleId)
        {
            if (PersonalVehicle == null)
            {
                API.SetResourceKvpInt(PERSONAL_VEHICLE_KEY, vehicleId);
                PersonalVehicle = new Vehicle(vehicleId);
                Decorators.Set(vehicleId, Decorators.PLAYER_VEHICLE, true);
            }
            else if (PersonalVehicle.Handle != vehicleId)
            {
                API.SetResourceKvpInt(PERSONAL_VEHICLE_KEY, vehicleId);
                PersonalVehicle = new Vehicle(vehicleId);
                Decorators.Set(vehicleId, Decorators.PLAYER_VEHICLE, true);
            }
        }

        private void OnPoliceDuty(bool active, bool onduty, string job)
        {
            IsOnDuty = onduty;
            IsOfficer = (job == "police");

            if (IsOfficer)
            {
                Game.PlayerPed.RelationshipGroup = (uint)Collections.RelationshipHash.Cop;

                MarkerHandler.Init();
                MarkerArrest.Init();
            }
            else
            {
                Game.PlayerPed.RelationshipGroup = (uint)Collections.RelationshipHash.Player;
                MarkerHandler.Dispose();
                MarkerArrest.Dispose();
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
                    }
                }
            }

        }

        private void OnPlayerSpawned(dynamic spawnObject)
        {
        }

        private void OnPlayerPatrolZone(int zone)
        {
            PatrolZone = (PatrolZone)zone;
        }
    }
}
