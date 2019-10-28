using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Curiosity.Shared.Client.net.Enums.Patrol;
using Curiosity.Shared.Client.net;
using static CitizenFX.Core.Native.API;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Global.Shared.net;
using Newtonsoft.Json;
using Curiosity.Missions.Client.net.Classes.PlayerClient;

namespace Curiosity.Missions.Client.net.Scripts.Mission
{
    class RandomMissionHandler
    {
        static Client client = Client.GetInstance();

        static bool StopSpam = false;
        static bool HasAcceptedCallout = false;
        static bool IsRequestingCallout = false;

        static long TimeStampOfLastCallout;
        static int RandomTimeBetweenCallouts = Client.Random.Next(60000, 180000);
        static int PreviousMissionId = 0;

        static public bool IsOnDuty = false;
        static bool IsOnActiveCallout = false;
        static public PatrolZone patrolZone = PatrolZone.City;

        static public void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Interface:Duty", new Action<bool, bool, string>(OnDutyState));
            client.RegisterEventHandler("curiosity:Client:Police:PatrolZone", new Action<int>(OnPatrolZone));
            
            client.RegisterEventHandler("curiosity:Client:Mission:Start", new Action<string>(OnPlayerCanStartMission));
            client.RegisterEventHandler("curiosity:Client:Mission:NotAvailable", new Action(OnMissionNotAvailable));
        }

        static void OnMissionNotAvailable()
        {
            IsRequestingCallout = false;
        }

        static void OnPatrolZone(int zone)
        {
            patrolZone = (PatrolZone)zone;
        }

        static async void OnDutyState(bool active, bool onduty, string job)
        {
            if (job != "police")
            {
                // clean up and stop everything
                client.DeregisterTickHandler(OnGenerateRandomMission);
                
                if (IsOnActiveCallout)
                    CreateStoreMission.CleanUp(true);

                Log.Info($"JOB: {job}");

                return;
            }

            if (StopSpam) return;
            StopSpam = true;

            if (IsOnDuty != onduty)
            {
                SoundManager.PlayAudio($"RESIDENT/DISPATCH_INTRO_0{Client.Random.Next(1, 3)} REPORT_RESPONSE/REPORT_RESPONSE_COPY_0{Client.Random.Next(1, 5)}");
            }

            IsOnDuty = onduty;

            if (!onduty)
            {
                if (PreviousMissionId > 0)
                    Client.TriggerServerEvent("curiosity:Server:Missions:Ended", PreviousMissionId);

                client.DeregisterTickHandler(OnGenerateRandomMission);
            }

            if (!onduty && IsOnActiveCallout)
            {
                CreateStoreMission.CleanUp(true);
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
                }
            }

            await Client.Delay(3000);
            StopSpam = false;
        }

        internal static void SetIsOnActiveCallout(bool state)
        {
            IsOnActiveCallout = state;

            if (IsOnActiveCallout)
            {
                client.DeregisterTickHandler(OnGenerateRandomMission);
            }
        }

        static public void AllowNextMission()
        {
            if (PreviousMissionId > 0)
                Client.TriggerServerEvent("curiosity:Server:Missions:Ended", PreviousMissionId);

            SetIsOnActiveCallout(false);
            RandomTimeBetweenCallouts = Client.Random.Next(60000, 180000);
            TimeStampOfLastCallout = GetGameTimer();

            IsRequestingCallout = false;

            client.RegisterTickHandler(OnGenerateRandomMission);
        }

        static async Task OnGenerateRandomMission()
        {
            if (!IsOnActiveCallout)
            {
                HasAcceptedCallout = false;

                if (!ClientInformation.IsDeveloper())
                {
                    if ((GetGameTimer() - TimeStampOfLastCallout) < RandomTimeBetweenCallouts)
                    {
                        if (ClientInformation.IsDeveloper())
                        {
                            Log.Info($"Waiting to create mission {RandomTimeBetweenCallouts}");
                            Log.Info($"Waiting to create mission {(GetGameTimer() - TimeStampOfLastCallout)}");
                        }

                        await Client.Delay(10000);
                        return;
                    }
                }

                if (IsRequestingCallout)
                {
                    await Client.Delay(10000);
                    return;
                }

                IsRequestingCallout = true;
                GenerateRandomMission();
            }
        }

        static void GenerateRandomMission()
        {
            if (Client.Random.Next(20) == 1)
            {
                ChooseRandomMissionAsync(DataClasses.Mission.PoliceStores.storesRural);
                return;
            }

            switch(patrolZone)
            {
                case PatrolZone.Country:
                    ChooseRandomMissionAsync(DataClasses.Mission.PoliceStores.storesCountry);
                    break;
                default: // CITY
                    ChooseRandomMissionAsync(DataClasses.Mission.PoliceStores.storesCity);
                    break;
            }
        }

        static async void ChooseRandomMissionAsync(Dictionary<int, DataClasses.Mission.Store> missions)
        {
            int randomMissionNumber = missions.Count == 1 ? 1 : Client.Random.Next(0, missions.Count);
            int missionId = missions.ElementAt(randomMissionNumber).Key;

            bool foundNewCallout = false;

            while (!foundNewCallout)
            {
                if (!IsOnDuty) return;
                randomMissionNumber = missions.Count == 1 ? 1 : Client.Random.Next(0, missions.Count);
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
                PatrolZone = (int)patrolZone
            };

            if (ClientInformation.IsDeveloper())
            {
                Log.Info($"Requesting Mission");
            }

            string message = Encode.StringToBase64(JsonConvert.SerializeObject(missionMessage));

            Client.TriggerServerEvent("curiosity:Server:Missions:Available", message);
        }

        static void OnPlayerCanStartMission(string missionData)
        {
            MissionCreate missionMessage = JsonConvert.DeserializeObject<MissionCreate>(Encode.Base64ToString(missionData));

            IsOnActiveCallout = true;
            PatrolZone missionPatrolZone = (PatrolZone)missionMessage.PatrolZone;

            DataClasses.Mission.Store mission = null;

            switch (missionPatrolZone)
            {
                case PatrolZone.Rural:
                    mission = DataClasses.Mission.PoliceStores.storesRural[missionMessage.MissionId];
                    break;
                case PatrolZone.Country:
                    mission = DataClasses.Mission.PoliceStores.storesCountry[missionMessage.MissionId];
                    break;
                default: // CITY
                    mission = DataClasses.Mission.PoliceStores.storesCity[missionMessage.MissionId];
                    break;
            }

            SetIsOnActiveCallout(true);

            CreateStoreMission.Create(mission);
        }
    }
}
