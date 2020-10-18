using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared;
using Curiosity.Global.Shared.Entity;
using Curiosity.Global.Shared.Enums;
using Curiosity.Missions.Client.Classes.PlayerClient;
using Curiosity.Missions.Client.Managers;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Enums.Patrol;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Missions.Client.Scripts
{
    class MissionEvents
    {
        static PluginManager PluginInstance => PluginManager.Instance;

        static long GameTime;
        static public bool HasAcceptedCallout = false;

        public static void Init()
        {
            PluginInstance.RegisterEventHandler("curiosity:Client:Mission:Start", new Action<string>(OnPlayerCanStartMission));
            PluginInstance.RegisterEventHandler("curiosity:Client:Mission:Dispatch", new Action<string>(OnDispatch));

            RegisterCommand("dispatch", new Action(OnDispatchRequest), false);
        }

        static void OnDispatchRequest()
        {
            PluginManager.TriggerServerEvent("curiosity:Server:Missions:Dispatch");
        }

        static void OnDispatch(string missionData)
        {
            ConcurrentDictionary<int, Tuple<string, int>> MissionsActive = JsonConvert.DeserializeObject<ConcurrentDictionary<int, Tuple<string, int>>>(Encode.Base64ToString(missionData));
            int CityDispatch = 0;
            int RuralDispatch = 0;
            int CountyDispatch = 0;

            foreach (KeyValuePair<int, Tuple<string, int>> keyValuePair in MissionsActive)
            {
                PatrolZone patrolZone = (PatrolZone)keyValuePair.Value.Item2;
                switch (patrolZone)
                {
                    case PatrolZone.City:
                        CityDispatch++;
                        break;
                    case PatrolZone.Rural:
                        RuralDispatch++;
                        break;
                    case PatrolZone.Country:
                        CountyDispatch++;
                        break;
                }
            }

            PluginManager.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Dispatch", $"Current Active", $"~b~{CityDispatch}/{DataClasses.Mission.PoliceCallouts.cityCallouts.Count} ~s~City~n~~b~{RuralDispatch}/{DataClasses.Mission.PoliceCallouts.ruralCallouts.Count} ~s~Rural~n~~b~{CountyDispatch}/{DataClasses.Mission.PoliceCallouts.countyCallouts.Count} ~s~Country", 2);
        }

        static async void OnPlayerCanStartMission(string missionData)
        {
            HasAcceptedCallout = false;
            GameTime = GetGameTimer();

            Mission.RandomMissionHandler.SetDispatchMessageRecieved(true);

            MissionCreate missionMessage = JsonConvert.DeserializeObject<MissionCreate>(Encode.Base64ToString(missionData));

            PluginManager.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Dispatch", $"Response Required", $"", 2);

            while (!HasAcceptedCallout)
            {
                DisableControlAction(0, (int)Control.FrontendDelete, true);
                DisableControlAction(0, (int)Control.FrontendAccept, true);

                await PluginManager.Delay(0);

                if ((GetGameTimer() - GameTime) > (1000 * 30))
                {
                    DeclineMission(missionMessage.MissionId);
                    return;
                }

                if (Game.IsDisabledControlPressed(0, Control.FrontendAccept))
                {

                    HasAcceptedCallout = true;
                    Screen.DisplayHelpTextThisFrame($"Callout Accepted");

                    EnableControlAction(0, (int)Control.FrontendDelete, true);
                    EnableControlAction(0, (int)Control.FrontendAccept, true);
                }

                if (Game.IsDisabledControlPressed(0, Control.FrontendDelete))
                {
                    DeclineMission(missionMessage.MissionId);
                    return;
                }

                if (!HasAcceptedCallout)
                    Screen.DisplayHelpTextThisFrame($"Press ~INPUT_FRONTEND_ACCEPT~ to accept callout, ~INPUT_FRONTEND_DELETE~ to decline.");
            }

            EnableControlAction(0, (int)Control.FrontendDelete, true);
            EnableControlAction(0, (int)Control.FrontendAccept, true);

            if (PlayerManager.IsDeveloper)
            {
                Log.Info($"Mission Message {missionMessage}");
            }

            PatrolZone missionPatrolZone = (PatrolZone)missionMessage.PatrolZone;

            Dictionary<int, DataClasses.Mission.MissionData> missions;

            switch (missionPatrolZone)
            {
                case PatrolZone.Rural:
                    missions = DataClasses.Mission.PoliceCallouts.ruralCallouts;
                    break;
                case PatrolZone.Country:
                    missions = DataClasses.Mission.PoliceCallouts.countyCallouts;
                    break;
                default: // CITY
                    missions = DataClasses.Mission.PoliceCallouts.cityCallouts;
                    break;
            }

            DataClasses.Mission.MissionData mission = missions[missionMessage.MissionId];

            PluginManager.TriggerServerEvent("curiosity:Server:Missions:StartedMission", missionMessage.MissionId);

            switch (mission.MissionType)
            {
                case MissionType.STOLEN_VEHICLE:
                    Mission.StolenVehicle.Create(mission);
                    break;
                default:
                    Mission.CreateStoreMission.Create(mission);
                    break;
            }

        }

        static void DeclineMission(int missionId)
        {
            Mission.RandomMissionHandler.AllowNextMission();
        }
    }
}
