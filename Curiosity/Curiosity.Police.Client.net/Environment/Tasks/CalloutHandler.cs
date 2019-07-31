﻿using Curiosity.Shared.Client.net.Enums.Patrol;
using Curiosity.Shared.Client.net;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core.Native;

namespace Curiosity.Police.Client.net.Environment.Tasks
{
    class CalloutHandler
    {
        static int FIVE_MINUTES = ((1000 * 60) * 3);
        // static int FIVE_MINUTES = 15000; // DEV USE
        static Client client = Client.GetInstance();
        static Random random = new Random();
        static int PreviousCallout = -1;
        static bool IsRunnningCallout = false;
        static bool TickIsRegistered = false;

        static long TimeStampOfLastCallout;

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Police:CalloutTaken", new Action(CalloutTaken));
            client.RegisterEventHandler("curiosity:Client:Police:CalloutEnded", new Action(PlayerCanTakeCallout));
            client.RegisterEventHandler("curiosity:Client:Police:CalloutStart", new Action<int, int>(CalloutStart));
        }

        static void CalloutStart(int calloutId, int patrolZone)
        {

            PatrolZone pz = (PatrolZone)patrolZone;

            if (pz == PatrolZone.Rural) // 50/50 chance of being called out to the middle of the map
            {
                ClassLoader.RuralCallOuts[calloutId].Invoke();
            }

            if (pz == PatrolZone.City)
            {
                ClassLoader.CityCallOuts[calloutId].Invoke();
            }

            if (pz == PatrolZone.Country)
            {
                ClassLoader.CountryCallOuts[calloutId].Invoke();
            }
        }

        static void CalloutTaken()
        {
            GetRandomCallout();
        }

        public static void CalloutEnded()
        {
            Client.TriggerServerEvent("curiosity:Server:Police:CalloutEnded", PreviousCallout);
        }

        public static async void PlayerCanTakeCallout()
        {
            //if (Classes.Player.PlayerInformation.IsDeveloper())
            //    Log.Verbose($"TICK: PlayerCanTakeCallout");

            if (TickIsRegistered) return;

            await Client.Delay(0);
            IsRunnningCallout = false;
            TimeStampOfLastCallout = API.GetGameTimer();

            FIVE_MINUTES = random.Next(60000, 300000);

            client.RegisterTickHandler(SelectCallout);
            TickIsRegistered = true;
        }

        public static async void PlayerIsOnActiveCalloutOrOffDuty()
        {
            //if (Classes.Player.PlayerInformation.IsDeveloper())
            //    Log.Verbose($"TICK: PlayerIsOnActiveCalloutOrOffDuty");

            await Client.Delay(0);
            TickIsRegistered = false;
            client.DeregisterTickHandler(SelectCallout);
        }

        static async Task SelectCallout()
        {
            try
            {
                if (!Job.DutyManager.IsPoliceJobActive)
                {
                    await Task.FromResult(0);
                    return;
                }

                if ((API.GetGameTimer() - TimeStampOfLastCallout) < FIVE_MINUTES)
                {
                    if (Job.DutyManager.IsOnCallout)
                    {
                        TimeStampOfLastCallout = API.GetGameTimer();
                    }

                    //if (Classes.Player.PlayerInformation.IsDeveloper())
                    //    Log.Verbose($"SelectCallout TimerValue: {(API.GetGameTimer() - TimeStampOfLastCallout) / 1000.0}s");

                    await Client.Delay(10000);
                    await Task.FromResult(0);
                    return;
                }

                if (IsRunnningCallout)
                {
                    await Task.FromResult(0);
                    return;
                };

                IsRunnningCallout = true;

                await Task.FromResult(0);

                GetRandomCallout();
            }
            catch (Exception ex)
            {
                // nothing
            }
        }

        static void GetRandomCallout()
        {
            if (random.Next(1) == 1) // 50/50 chance of being called out to the middle of the map
            {
                GetRandomCallout(ClassLoader.RuralCallOuts, PatrolZone.Rural);
            }

            if (Job.DutyManager.PatrolZone == PatrolZone.City)
            {

                GetRandomCallout(ClassLoader.CityCallOuts, PatrolZone.City);
            }

            if (Job.DutyManager.PatrolZone == PatrolZone.Country)
            {
                GetRandomCallout(ClassLoader.CountryCallOuts, PatrolZone.Country);
            }
        }

        static async void GetRandomCallout(Dictionary<int, Func<bool>> calloutDictionary, PatrolZone patrolZone)
        {
            client.DeregisterTickHandler(SelectCallout);

            //if (Classes.Player.PlayerInformation.IsDeveloper())
            //    Log.Verbose($"GetRandomCallout");

            int randomCalloutIndex = random.Next(0, calloutDictionary.Count);
            int calloutId = calloutDictionary.ElementAt(randomCalloutIndex).Key;

            bool foundNewCallout = false;

            while (!foundNewCallout)
            {
                randomCalloutIndex = random.Next(0, calloutDictionary.Count);
                calloutId = calloutDictionary.ElementAt(randomCalloutIndex).Key;

                if (PreviousCallout != calloutId)
                {
                    foundNewCallout = true;
                }

                await Client.Delay(100);
            }

            PreviousCallout = calloutId;

            //if (Classes.Player.PlayerInformation.IsDeveloper())
            //    Log.Verbose($"GetRandomCallout: Result {calloutId}");

            Client.TriggerServerEvent("curiosity:Server:Police:CalloutFree", calloutId, (int)patrolZone);

            //calloutDictionary.ElementAt(randomCalloutIndex).Value.Invoke();

            await Task.FromResult(0);
        }
    }
}
