using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Ped = Curiosity.MissionManager.Client.Classes.Ped;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.StolenVehicle.Missions
{
    [MissionInfo("City Store Robery", "misSrCity", 0f, 0f, 0f, MissionType.Store, true, "None", PatrolZone.City)]
    public class CityStores : Mission
    {
        Dictionary<string, Tuple<Vector3, float>> storeClerkSpawns = new Dictionary<string, Tuple<Vector3, float>>()
        {
            { "city1", new Tuple<Vector3, float>(new Vector3(376.4324f, 321.6145f, 103.4308f), 172.1828f) },
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

                storeClerkBlip = storeClerk.AttachBlip();
                storeClerkBlip.Sprite = BlipSprite.Information;
                storeClerkBlip.IsShortRange = false;
                storeClerkBlip.ShowRoute = true;


                await BaseScript.Delay(100);

                

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

    internal enum MissionState
    {
        Started,
        End,
        SpokenToClerk,
        SetupSuspectLocation,
        LookingForSuspect,
        SuspectFlee
    }
}
