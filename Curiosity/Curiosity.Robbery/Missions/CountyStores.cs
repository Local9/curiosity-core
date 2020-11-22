﻿using CitizenFX.Core;
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
    [MissionInfo("County Store Robery", "misSrCounty", 0f, 0f, 0f, MissionType.Store, true, "None", PatrolZone.County)]
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

        Ped storeClerk;
        Ped thief;

        PedHash storeClerkHash = PedHash.ShopKeep01;

        public async override void Start()
        {
            try
            {
                missionState = MissionState.Started;

                var randomStore = storeClerkSpawns.ToList()[Utility.RANDOM.Next(storeClerkSpawns.Count)];

                if (randomStore.Value == null)
                {
                    Stop(EndState.Error);
                    return;
                }

                Vector3 storeClerkPosition = randomStore.Value.Item1;
                float storeClerkHeading = randomStore.Value.Item2;

                storeClerk = await Ped.Spawn(storeClerkHash, storeClerkPosition, storeClerkHeading, false, PedType.PED_TYPE_SPECIAL, false, true);

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

                Notify.DispatchAI("Report of Shoplifting", "Report of a shiplifter, please goto location to gain information.");

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

                            missionState = MissionState.SetupSuspectLocation;
                        }
                    }

                    break;
                case MissionState.SetupSuspectLocation:

                    thief = await Ped.SpawnRandom(storeClerk.Position.AroundStreet(200f, 400f), isNetworked: false);

                    if (thief == null)
                    {
                        Stop(EndState.Error);
                        return;
                    }

                    thief.IsImportant = true;
                    thief.IsMission = true;
                    thief.IsSuspect = true;

                    thief.Fx.Task.WanderAround(thief.Position, 20f);

                    Blip blip = World.CreateBlip(thief.Position.Around(10f, 20f));
                    blip.Sprite = BlipSprite.BigCircle;
                    blip.Scale = 0.5f;
                    blip.Color = (BlipColor)5;
                    blip.Alpha = 126;
                    blip.ShowRoute = false;
                    blip.Priority = 9;
                    blip.IsShortRange = true;

                    RegisterBlip(blip);

                    missionState = MissionState.LookingForSuspect;

                    break;
                case MissionState.LookingForSuspect:

                    if (Game.PlayerPed.Position.Distance(thief.Position) < 10f)
                    {
                        thief.Task.ClearAllImmediately();
                        thief.Task.ReactAndFlee(Game.PlayerPed);

                        Blip b = thief.AttachBlip();
                        b.Sprite = BlipSprite.Enemy;
                        b.Color = BlipColor.Red;
                        b.Priority = 9;

                        missionState = MissionState.SuspectFlee;
                    }

                    break;
            }
        }
    }
}
