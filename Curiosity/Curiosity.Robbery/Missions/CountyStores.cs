using CitizenFX.Core;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Ped = Curiosity.MissionManager.Client.Classes.Ped;

namespace Curiosity.StolenVehicle.Missions
{
    [MissionInfo("County Store Robbery", "misSrCounty", 0f, 0f, 0f, MissionType.Store, true, "None", PatrolZone.County)]
    public class CountyStores : Mission
    {
        Dictionary<string, Tuple<Vector3, float>> storeClerkSpawns = new Dictionary<string, Tuple<Vector3, float>>()
        {
            { "county1", new Tuple<Vector3, float>(new Vector3(1965.702f, 3739.749f, 31.31993f), 212.7391f) },
            { "county2", new Tuple<Vector3, float>(new Vector3(1166.458f, 2702.856f, 37.17479f), 179.9841f) },
            { "county3", new Tuple<Vector3, float>(new Vector3(2683.345f, 3281.904f, 54.24051f), 234.5024f) },
            { "county4", new Tuple<Vector3, float>(new Vector3(1698.414f, 4930.02f, 41.0781f), 39.50763f) },
            { "county5", new Tuple<Vector3, float>(new Vector3(543.7537f, 2674.644f, 41.15426f), 19.35232f) },
            { "county6", new Tuple<Vector3, float>(new Vector3(1730.481f, 6410.729f, 34.00061f), 159.2767f) },
            { "county7", new Tuple<Vector3, float>(new Vector3(1394.649f, 3598.51f, 33.98967f), 198.4347f) },
        };

        MissionState missionState;

        Blip storeClerkBlip;
        Blip locationBlip;

        Ped storeClerk;
        Ped thief;

        PedHash storeClerkHash = PedHash.ShopKeep01;

        public async override void Start()
        {
            try
            {
                missionState = MissionState.Started;

                var randomStore = storeClerkSpawns.ToList()[Utility.RANDOM.Next(storeClerkSpawns.Count)];

                DiscordStatus("Traveling to the scene");

                if (randomStore.Value == null)
                {
                    Stop(EndState.Error);
                    return;
                }

                Vector3 storeClerkPosition = randomStore.Value.Item1;
                float storeClerkHeading = randomStore.Value.Item2;

                Vector3 location = storeClerkPosition;

                Blip locationBlip = Functions.SetupLocationBlip(location);
                RegisterBlip(locationBlip);

                while (location.Distance(Game.PlayerPed.Position) > 50f)
                {
                    await BaseScript.Delay(100);
                }

                if (locationBlip.Exists())
                    locationBlip.Delete();

                if (storeClerk == null)
                    storeClerk = await PedSpawn(storeClerkHash, storeClerkPosition, storeClerkHeading, false, PedType.PED_TYPE_SPECIAL);

                if (storeClerk == null)
                {
                    Stop(EndState.Error);
                    return;
                }

                storeClerk.IsImportant = true;
                storeClerk.IsPositionFrozen = true;
                storeClerk.IsCollisionEnabled = false;
                storeClerk.IsInvincible = true;

                storeClerkBlip = storeClerk.AttachBlip();
                storeClerkBlip.Sprite = BlipSprite.Information;
                storeClerkBlip.IsShortRange = false;
                storeClerkBlip.ShowRoute = true;

                RegisteredPeds.Add(storeClerk);

                Notify.DispatchAI("Shoplifter", "Speak with the store clerk to get more information.");

                MissionManager.Instance.RegisterTickHandler(OnMissionTick);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    Debug.WriteLine($"Inner Exception: {ex.InnerException}");

                Debug.WriteLine($"Mission Start: {ex}");
            }
        }

        public override void End()
        {
            MissionManager.Instance.DeregisterTickHandler(OnMissionTick);
        }

        async Task OnMissionTick()
        {
            switch(missionState)
            {
                case MissionState.Started:

                    if (Game.PlayerPed.Position.DistanceTo(storeClerk.Position) < 2f)
                    {
                        HelpMessage.CustomLooped(HelpMessage.Label.MISSION_CLERK_SPEAK_WITH);

                        if (storeClerk.AttachedBlip != null)
                        {
                            if (storeClerk.AttachedBlip.Exists())
                            {
                                storeClerk.AttachedBlip.Delete();
                            }
                        }

                        if (Game.IsControlJustPressed(0, Control.Context))
                        {
                            while (Game.PlayerPed.Position.DistanceTo(storeClerk.Position) < 2f)
                            {
                                HelpMessage.CustomLooped(HelpMessage.Label.MISSION_CLERK_RESPONSE_SUSPECT_RAN);
                                await BaseScript.Delay(0);
                            }

                            storeClerk.Dismiss();

                            missionState = MissionState.SetupSuspectLocation;
                        }
                    }

                    break;
                case MissionState.SetupSuspectLocation:

                    Vector3 spawnLocation = storeClerk.Position.AroundStreet(200f, 400f);

                    if (spawnLocation == Vector3.Zero)
                    {
                        spawnLocation = storeClerk.Position.Around(200f, 400f);
                    }

                    locationBlip = Functions.SetupLocationBlip(spawnLocation.Around(10f, 20f));
                    RegisterBlip(locationBlip);

                    while (spawnLocation.Distance(Game.PlayerPed.Position) > 50f)
                    {
                        await BaseScript.Delay(100);
                    }

                    if (locationBlip.Exists())
                        locationBlip.Delete();

                    thief = await PedSpawnRandom(spawnLocation);

                    if (thief == null)
                    {
                        Stop(EndState.Error);
                        return;
                    }

                    thief.AttachSuspectBlip();

                    thief.IsImportant = true;
                    thief.IsMission = true;
                    thief.IsSuspect = true;
                    thief.IsArrestable = true;

                    thief.Fx.Task.WanderAround(thief.Position, 20f);

                    RegisteredPeds.Add(thief);

                    locationBlip = Functions.SetupLocationBlip(thief.Position.Around(10f, 20f));
                    RegisterBlip(locationBlip);

                    missionState = MissionState.LookingForSuspect;

                    DiscordStatus("Looking for a suspect");

                    break;
                case MissionState.LookingForSuspect:

                    if (Game.PlayerPed.Position.Distance(thief.Position) < 10f)
                    {
                        if (locationBlip.Exists())
                            locationBlip.Delete();

                        thief.Task.ClearAllImmediately();
                        thief.Task.ReactAndFlee(Game.PlayerPed);

                        missionState = MissionState.SuspectFlee;
                    }

                    break;
            }

            if (NumberPedsArrested > 0)
                Pass();

            if (NumberPedsArrested == 0)
            {
                if (thief != null && thief.IsDead)
                    Fail("Suspect is dead");
            }
        }
    }
}
