using System;
using System.Collections.Generic;
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

namespace Curiosity.Missions.Client.net.Scripts.Mission
{
    class RandomMissionHandler
    {
        static Client client = Client.GetInstance();

        static bool StopSpam = false;
        static bool HasAcceptedCallout = false;
        static bool IsRequestingCallout = false;

        static long TimeStampOfLastCallout;
        static long GameTime;
        static int RandomTimeBetweenCallouts = Client.Random.Next(60000, 180000);
        static int PreviousMissionId = 0;

        static public bool IsOnDuty = false;
        static bool IsOnActiveCallout = false;
        static public PatrolZone patrolZone = PatrolZone.City;

        static public void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Interface:Duty", new Action<bool, bool, string>(OnDutyState));
            client.RegisterEventHandler("curiosity:Client:Police:PatrolZone", new Action<int>(OnPatrolZone));
           
            client.RegisterEventHandler("curiosity:Client:Mission:NotAvailable", new Action(OnMissionNotAvailable));

            client.RegisterEventHandler("curiosity:Client:Missions:MissionComplete", new Action(OnMissionComplete));
        }

        static void OnMissionComplete()
        {
            SoundManager.PlayAudio($"RESIDENT/DISPATCH_INTRO_0{Client.Random.Next(1, 3)} REPORT_RESPONSE/REPORT_RESPONSE_COPY_0{Client.Random.Next(1, 5)}");
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
                }
            }

            await Client.Delay(3000);
            StopSpam = false;
        }

        internal static void SetIsOnActiveCallout(bool state)
        {
            IsOnActiveCallout = state;
            if (IsOnActiveCallout)
                client.DeregisterTickHandler(OnGenerateRandomMission);
        }

        static public void AllowNextMission()
        {
            if (PreviousMissionId > 0)
                Client.TriggerServerEvent("curiosity:Server:Missions:Ended", PreviousMissionId);

            SetIsOnActiveCallout(false);
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

                if (IsRequestingCallout)
                {
                    GameTime = GetGameTimer();

                    Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Dispatch", $"Response Required", $"", 2);

                    while (!HasAcceptedCallout)
                    {
                        DisableControlAction(0, (int)Control.FrontendDelete, true);
                        DisableControlAction(0, (int)Control.FrontendAccept, true);

                        await Client.Delay(0);

                        if ((GetGameTimer() - GameTime) > (1000 * 30))
                        {
                            IsRequestingCallout = false;
                            RandomTimeBetweenCallouts = Client.Random.Next(60000, 120000);
                            return;
                        }

                        if (Game.IsDisabledControlPressed(0, Control.FrontendAccept))
                        {

                            HasAcceptedCallout = true;
                            Screen.DisplayHelpTextThisFrame($"Callout Accepted");
                            ChoseRandomMissionArea();

                            EnableControlAction(0, (int)Control.FrontendDelete, true);
                            EnableControlAction(0, (int)Control.FrontendAccept, true);

                            return;
                        }

                        if (Game.IsDisabledControlPressed(0, Control.FrontendDelete))
                        {
                            AllowNextMission();
                            await Client.Delay(1000);
                            return;
                        }

                        if (!HasAcceptedCallout)
                            Screen.DisplayHelpTextThisFrame($"Press ~INPUT_FRONTEND_ACCEPT~ to accept callout, ~INPUT_FRONTEND_DELETE~ to decline.");
                    }

                    EnableControlAction(0, (int)Control.FrontendDelete, true);
                    EnableControlAction(0, (int)Control.FrontendAccept, true);

                    return;
                }

                IsRequestingCallout = true;

                SoundManager.PlayAudio($"RESIDENT/DISPATCH_INTRO_0{Client.Random.Next(1, 3)} ASSISTANCE_REQUIRED/ASSISTANCE_REQUIRED_0{Client.Random.Next(1, 5)} UNITS_RESPOND/UNITS_RESPOND_CODE_02_0{Client.Random.Next(1, 3)}");
            }
        }

        static async Task ChoseRandomMissionArea()
        {
            HasAcceptedCallout = true;

            if (Client.Random.Next(20) == 1 && DataClasses.Mission.PoliceStores.storesRural.Count > 0)
            {
                ChooseRandomMissionAsync(DataClasses.Mission.PoliceStores.storesRural, PatrolZone.Rural);
                return;
            }

            switch (patrolZone)
            {
                case PatrolZone.Country:
                    ChooseRandomMissionAsync(DataClasses.Mission.PoliceStores.storesCountry, PatrolZone.Country);
                    break;
                default: // CITY
                    ChooseRandomMissionAsync(DataClasses.Mission.PoliceStores.storesCity, PatrolZone.City);
                    break;
            }
        }

        static async Task ChooseRandomMissionAsync(Dictionary<int, DataClasses.Mission.Store> missions, PatrolZone missionPatrolZone)
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
