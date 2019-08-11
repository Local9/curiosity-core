using Curiosity.Shared.Client.net.Enums.Patrol;
using Curiosity.Shared.Client.net;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core.Native;
using CitizenFX.Core;

namespace Curiosity.Police.Client.net.Environment.Tasks
{
    class CalloutHandler
    {
        static int FIVE_MINUTES = ((1000 * 60) * 3);
        static Client client = Client.GetInstance();
        static Random random = new Random();
        static int PreviousCallout = -1;
        static bool TickIsRegistered = false;

        static long TimeStampOfLastCallout;
        public static string DEV_LICENSE_PLATE = "LIFEVDEV";

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Police:CalloutTaken", new Action(CalloutTaken));
            client.RegisterEventHandler("curiosity:Client:Police:CalloutEnded", new Action(PlayerCanTakeCallout));
            client.RegisterEventHandler("curiosity:Client:Police:CalloutStart", new Action<int, int>(CalloutStart));
            client.RegisterEventHandler("onClientResourceStop", new Action<string>(OnClientResourceStop));
        }

        static void OnClientResourceStop(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;
            Client.TriggerServerEvent("curiosity:Server:Police:CalloutEnded", PreviousCallout);
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
            try
            {
                //if (Classes.Player.PlayerInformation.IsDeveloper())
                //    Log.Verbose($"TICK: PlayerCanTakeCallout");

                if (TickIsRegistered) return;
                TickIsRegistered = true;

                Client.TriggerServerEvent("curiosity:Server:Police:CalloutEnded", PreviousCallout);

                await Client.Delay(0);
                Job.DutyManager.IsOnCallout = false;
                TimeStampOfLastCallout = API.GetGameTimer();

                if (Classes.Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.DEVELOPER)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        if (Game.PlayerPed.CurrentVehicle.Mods.LicensePlate == DEV_LICENSE_PLATE)
                        {
                            FIVE_MINUTES = 5000;
                        }
                    }
                }
                else
                {
                    FIVE_MINUTES = random.Next(60000, 180000);
                }

                client.RegisterTickHandler(SelectCallout);
            }
            catch (Exception ex)
            {
                Log.Error($"PlayerCanTakeCallout -> {ex.Message}");
            }
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

                if (Job.DutyManager.IsOnCallout)
                {
                    await Task.FromResult(0);
                    return;
                };

                Job.DutyManager.IsOnCallout = true;

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
            if (random.Next(9) == 1) // 1/10 chance of being called out to the middle of the map
            {
                GetRandomCallout(ClassLoader.RuralCallOuts, PatrolZone.Rural);
                return;
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
