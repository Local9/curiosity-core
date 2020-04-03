using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Curiosity.Shared.Client.net.Enums.Patrol;
using Curiosity.Shared.Client.net;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Global.Shared.net;
using Newtonsoft.Json;
using Curiosity.Missions.Client.net.Classes.PlayerClient;
using Curiosity.Shared.Client.net.Enums;

namespace Curiosity.Missions.Client.net.Scripts
{
    class MissionEvents
    {
        static Client client = Client.GetInstance();

        static long GameTime;
        static public bool HasAcceptedCallout = false;

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Mission:Start", new Action<string>(OnPlayerCanStartMission));
            client.RegisterEventHandler("curiosity:Client:Mission:Dispatch", new Action<string>(OnDispatch));

            RegisterCommand("dispatch", new Action(OnDispatchRequest), false);
        }

        static void OnDispatchRequest()
        {
            Client.TriggerServerEvent("curiosity:Server:Missions:Dispatch");
        }

        static void OnDispatch(string missionData)
        {
            ConcurrentDictionary<int, Tuple<string, int>> MissionsActive = JsonConvert.DeserializeObject <ConcurrentDictionary<int, Tuple<string, int>>>(Encode.Base64ToString(missionData));
            int CityDispatch = 0;
            int RuralDispatch = 0;
            int CountyDispatch = 0;

            foreach(KeyValuePair<int, Tuple<string, int>> keyValuePair in MissionsActive)
            {
                PatrolZone patrolZone = (PatrolZone)keyValuePair.Value.Item2;
                switch(patrolZone)
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

            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Dispatch", $"Current Active", $"~b~{CityDispatch}/{DataClasses.Mission.PoliceStores.storesCity.Count} ~s~City~n~~b~{RuralDispatch}/{DataClasses.Mission.PoliceStores.storesRural.Count} ~s~Rural~n~~b~{CountyDispatch}/{DataClasses.Mission.PoliceStores.storesCountry.Count} ~s~Country", 2);
        }

        static async void OnPlayerCanStartMission(string missionData)
        {
            HasAcceptedCallout = false;
            GameTime = GetGameTimer();

            Mission.RandomMissionHandler.SetDispatchMessageRecieved(true);

            MissionCreate missionMessage = JsonConvert.DeserializeObject<MissionCreate>(Encode.Base64ToString(missionData));

            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Dispatch", $"Response Required", $"", 2);

            while (!HasAcceptedCallout)
            {
                DisableControlAction(0, (int)Control.FrontendDelete, true);
                DisableControlAction(0, (int)Control.FrontendAccept, true);

                await Client.Delay(0);

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

            if (ClientInformation.IsDeveloper())
            {
                Log.Info($"Mission Message {missionMessage}");
            }

            PatrolZone missionPatrolZone = (PatrolZone)missionMessage.PatrolZone;

            Dictionary<int, DataClasses.Mission.MissionData> missions = new Dictionary<int, DataClasses.Mission.MissionData>();

            switch (missionPatrolZone)
            {
                case PatrolZone.Rural:
                    missions = DataClasses.Mission.PoliceStores.storesRural;
                    break;
                case PatrolZone.Country:
                    missions = DataClasses.Mission.PoliceStores.storesCountry;
                    break;
                default: // CITY
                    missions = DataClasses.Mission.PoliceStores.storesCity;
                    break;
            }

            DataClasses.Mission.MissionData mission = missions[missionMessage.MissionId];

            Client.TriggerServerEvent("curiosity:Server:Missions:StartedMission", missionMessage.MissionId);

            Mission.CreateStoreMission.Create(mission);
        }

        static void DeclineMission(int missionId)
        {
            Mission.RandomMissionHandler.AllowNextMission();
        }
    }
}
