﻿using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared;
using Curiosity.Global.Shared.Entity;
using Curiosity.Missions.Client.net.Classes.PlayerClient;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Enums.Patrol;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Missions.Client.net.Scripts.Mission
{
    class RandomMissionHandler
    {
        static Client client = Client.GetInstance();

        static bool StopSpam = false;
        static public bool HasAcceptedCallout = false;
        static bool IsRequestingCallout = false;

        static long TimeStampOfLastCallout;

        static public int RandomTimeBetweenCallouts = Client.Random.Next(60000, 180000);
        static int PreviousMissionId = 0;

        static public bool IsOnDuty = false;
        static bool IsOnActiveCallout = false;
        static public PatrolZone patrolZone = PatrolZone.City;

        static bool _IsArrestActive = false;
        static bool _IsRandomEventsActive = false;
        static bool _IsTrafficStopActive = false;

        static public string JobName = string.Empty;

        static public void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Interface:Duty", new Action<bool, bool, string>(OnDutyState));
            client.RegisterEventHandler("curiosity:Client:Mission:TrafficStops", new Action<bool>(OnTrafficStops));
            client.RegisterEventHandler("curiosity:Client:Mission:RandomEvents", new Action<bool>(OnRandomEvents));
            client.RegisterEventHandler("curiosity:Client:Police:PatrolZone", new Action<int>(OnPatrolZone));
            client.RegisterEventHandler("curiosity:Client:Mission:NotAvailable", new Action(OnMissionNotAvailable));
            client.RegisterEventHandler("curiosity:Client:Missions:MissionComplete", new Action(OnMissionComplete));
        }

        static void OnRandomEvents(bool state)
        {
            _IsRandomEventsActive = state;
            if (state)
            {
                Police.RandomCallouts.Setup();
            }
            else
            {
                Police.RandomCallouts.Dispose();
            }
        }

        static void OnTrafficStops(bool state)
        {
            _IsTrafficStopActive = state;

            if (Client.CurrentVehicle == null)
            {
                Screen.ShowNotification("~b~Traffic Stops: ~r~You currently do not have a Personal Vehicle for Traffic Stops.");
                Client.TriggerEvent("curiosity:Client:Police:TrafficStops", false);
                return;
            }

            if (Game.PlayerPed.CurrentVehicle != Client.CurrentVehicle)
            {
                Screen.ShowNotification("~b~Traffic Stops: ~r~This vehicle is not your personal vehicle.");
                Client.TriggerEvent("curiosity:Client:Police:TrafficStops", false);
                return;
            }

            if (state)
            {
                Client.TriggerEvent("curiosity:Client:Context:TrafficStopActive", true);
                Police.TrafficStop.Setup();
            }
            else
            {
                Client.TriggerEvent("curiosity:Client:Context:TrafficStopActive", false);
                Police.TrafficStop.Dispose();
            }
        }

        static void OnMissionComplete()
        {
            SoundManager.PlayAudio($"RESIDENT/DISPATCH_INTRO_0{Client.Random.Next(1, 3)} REPORT_RESPONSE/REPORT_RESPONSE_COPY_0{Client.Random.Next(1, 5)}");
        }

        static void OnMissionNotAvailable()
        {
            if (ClientInformation.IsDeveloper())
            {
                Log.Info($"Requesting Mission, none Available");
            }

            SetDispatchMessageRecieved(false);
            RandomTimeBetweenCallouts = Client.Random.Next(60000, 180000);
            TimeStampOfLastCallout = GetGameTimer();

            IsRequestingCallout = false;
            HasAcceptedCallout = false;
        }

        static void OnPatrolZone(int zone)
        {
            patrolZone = (PatrolZone)zone;
        }

        static async void OnDutyState(bool active, bool onduty, string job)
        {
            JobName = job;

            if (job != "police")
            {
                IsRequestingCallout = false;
                HasAcceptedCallout = false;

                // clean up and stop everything
                client.DeregisterTickHandler(OnGenerateRandomMission);

                if (IsOnActiveCallout)
                    CreateStoreMission.CleanUp(true);

                Log.Info($"JOB: {job}");

                Police.ArrestPed.Dispose();

                if (_IsRandomEventsActive)
                    Police.RandomCallouts.Dispose();

                if (_IsTrafficStopActive)
                    Police.TrafficStop.Dispose();

                if (Extras.VehicleTow.IsServiceActive)
                    Scripts.Extras.VehicleTow.Reset();

                if (Extras.Coroner.IsServiceActive)
                    Scripts.Extras.Coroner.Reset();

                return;
            }

            if (StopSpam) return;
            StopSpam = true;

            if (IsOnDuty != onduty)
            {
                SoundManager.PlayAudio($"RESIDENT/DISPATCH_INTRO_0{Client.Random.Next(1, 3)} REPORT_RESPONSE/REPORT_RESPONSE_COPY_0{Client.Random.Next(1, 5)}");
            }

            IsOnDuty = onduty;

            Police.ArrestPed.Setup();

            if (!onduty)
            {
                if (PreviousMissionId > 0)
                    Client.TriggerServerEvent("curiosity:Server:Missions:Ended", PreviousMissionId);

                client.DeregisterTickHandler(OnGenerateRandomMission);
            }

            if (!onduty && IsOnActiveCallout)
            {
                CreateStoreMission.CleanUp(true);
                client.DeregisterTickHandler(OnGenerateRandomMission);
            }
            else
            {
                if (onduty)
                {
                    IsRequestingCallout = false;
                    IsOnActiveCallout = false;
                    TimeStampOfLastCallout = GetGameTimer();
                    client.RegisterTickHandler(OnGenerateRandomMission);
                    Log.Info($"Player is on duty");
                    Static.Relationships.Init();
                }
            }

            await Client.Delay(3000);
            StopSpam = false;
        }

        internal static void SetDispatchMessageRecieved(bool state)
        {
            IsOnActiveCallout = state;
            if (IsOnActiveCallout)
                client.DeregisterTickHandler(OnGenerateRandomMission);
        }

        static public void AllowNextMission()
        {
            if (!IsOnDuty) return;

            if (PreviousMissionId > 0)
            {
                Client.TriggerServerEvent("curiosity:Server:Missions:EndMission");
                Client.TriggerServerEvent("curiosity:Server:Missions:Ended", PreviousMissionId);
            }

            SetDispatchMessageRecieved(false);
            RandomTimeBetweenCallouts = Client.Random.Next(60000, 180000);
            TimeStampOfLastCallout = GetGameTimer();

            IsRequestingCallout = false;
            HasAcceptedCallout = false;

            client.RegisterTickHandler(OnGenerateRandomMission);
        }

        static async Task OnGenerateRandomMission()
        {
            if (!IsOnActiveCallout && !HasAcceptedCallout)
            {
                while ((GetGameTimer() - TimeStampOfLastCallout) < RandomTimeBetweenCallouts)
                {
                    if (ClientInformation.IsDeveloper())
                    {
                        Log.Info($"Waiting to create mission {RandomTimeBetweenCallouts}");
                        Log.Info($"Waiting to create mission {(GetGameTimer() - TimeStampOfLastCallout)}");
                    }

                    await Client.Delay(10000);

                    if (!IsOnDuty)
                    {
                        SetDispatchMessageRecieved(false);

                        IsRequestingCallout = false;
                        HasAcceptedCallout = false;

                        client.DeregisterTickHandler(OnGenerateRandomMission);
                        return;
                    }
                }

                if (!IsRequestingCallout)
                {
                    ChoseRandomMissionArea();
                    IsRequestingCallout = true;
                }
            }
        }

        static async Task ChoseRandomMissionArea()
        {
            HasAcceptedCallout = true;

            if (Client.Random.Next(20) == 1 && DataClasses.Mission.PoliceCallouts.ruralCallouts.Count > 0)
            {
                ChooseRandomMissionAsync(DataClasses.Mission.PoliceCallouts.ruralCallouts, PatrolZone.Rural);
                return;
            }

            switch (patrolZone)
            {
                case PatrolZone.Country:
                    ChooseRandomMissionAsync(DataClasses.Mission.PoliceCallouts.countyCallouts, PatrolZone.Country);
                    break;
                default: // CITY
                    ChooseRandomMissionAsync(DataClasses.Mission.PoliceCallouts.cityCallouts, PatrolZone.City);
                    break;
            }
        }

        static async Task ChooseRandomMissionAsync(Dictionary<int, DataClasses.Mission.MissionData> missions, PatrolZone missionPatrolZone)
        {
            int randomMissionNumber = missions.Count == 1 ? 0 : Client.Random.Next(0, missions.Count);
            int missionId = missions.ElementAt(randomMissionNumber).Key;

            bool foundNewCallout = false;

            while (!foundNewCallout)
            {
                if (!IsOnDuty) return;
                randomMissionNumber = missions.Count == 1 ? 0 : Client.Random.Next(0, missions.Count);
                missionId = missions.ElementAt(randomMissionNumber).Key;

                if (missionId != PreviousMissionId)
                {
                    foundNewCallout = true;
                }

                await Client.Delay(1000);
            }

            PreviousMissionId = missionId;

            MissionCreate missionMessage = new MissionCreate()
            {
                MissionId = missionId,
                PatrolZone = (int)missionPatrolZone
            };

            if (ClientInformation.IsDeveloper())
            {
                Log.Info($"Requesting Mission");
            }

            string message = Encode.StringToBase64(JsonConvert.SerializeObject(missionMessage));

            Client.TriggerServerEvent("curiosity:Server:Missions:Available", message);
        }
    }
}
