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
        static bool RequestingCallout = false;

        static long TimeStampOfLastCallout;
        public static string DEV_LICENSE_PLATE = "LIFEVDEV";

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Police:CalloutTaken", new Action(CalloutTaken));
            client.RegisterEventHandler("curiosity:Client:Police:PlayerCanTakeCallout", new Action(PlayerCanTakeCallout));
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
            Job.DutyManager.IsOnCallout = true;

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
            RequestingCallout = false;
            GenerateRandomCallout();
        }

        public static void CalloutEnded()
        {
            Client.TriggerServerEvent("curiosity:Server:Police:CalloutEnded", PreviousCallout);
        }

        public static async void PlayerCanTakeCallout()
        {
            try
            {
                if (TickIsRegistered) return;
                TickIsRegistered = true;

                RequestingCallout = false;

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
            await Client.Delay(0);
            TickIsRegistered = false;
            RequestingCallout = false;
            client.DeregisterTickHandler(SelectCallout);
        }

        public static async Task SelectCallout()
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

                    await Client.Delay(10000);
                    await Task.FromResult(0);
                    return;
                }

                if (RequestingCallout)
                {
                    await Task.FromResult(0);
                    return;
                };

                await Task.FromResult(0);

                RequestingCallout = true;

                await GenerateRandomCallout();
            }
            catch (Exception ex)
            {
                if (Classes.Player.PlayerInformation.IsDeveloper())
                {
                    Log.Error($"SelectCallout -> {ex.Message}");
                }
            }
        }

        static async Task GenerateRandomCallout()
        {
            try
            {
                if (random.Next(10) == 1) // 1/10 chance of being called out to the middle of the map
                {
                    await GetRandomCallout(ClassLoader.RuralCallOuts, PatrolZone.Rural);
                    return;
                }

                if (Job.DutyManager.PatrolZone == PatrolZone.City)
                {
                    await GetRandomCallout(ClassLoader.CityCallOuts, PatrolZone.City);
                }

                if (Job.DutyManager.PatrolZone == PatrolZone.Country)
                {
                    await GetRandomCallout(ClassLoader.CountryCallOuts, PatrolZone.Country);
                }
                await Task.FromResult(0);
            }
            catch (Exception ex)
            {
                if (Classes.Player.PlayerInformation.IsDeveloper())
                {
                    Log.Error($"GenerateRandomCallout -> {ex.Message}");
                }
            }
        }

        static async Task GetRandomCallout(Dictionary<int, Func<bool>> calloutDictionary, PatrolZone patrolZone)
        {
            try
            { 
                client.DeregisterTickHandler(SelectCallout);
                int maxCallout = calloutDictionary.Count;

                int randomCalloutIndex = maxCallout == 1 ? 1 : random.Next(0, maxCallout);
                int calloutId = calloutDictionary.ElementAt(randomCalloutIndex).Key;

                bool foundNewCallout = false;

                while (!foundNewCallout)
                {
                    randomCalloutIndex = maxCallout == 1 ? 1 : random.Next(0, maxCallout);
                    calloutId = calloutDictionary.ElementAt(randomCalloutIndex).Key;

                    if (PreviousCallout != calloutId)
                    {
                        foundNewCallout = true;
                    }

                    await Client.Delay(100);
                }

                PreviousCallout = calloutId;

                Client.TriggerServerEvent("curiosity:Server:Police:CalloutFree", calloutId, (int)patrolZone);
            }
            catch (Exception ex)
            {
                if (Classes.Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.DEVELOPER)
                    Log.Error($"GetRandomCallout -> {ex.Message}");
            }
            await Task.FromResult(0);
        }
    }
}
