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
    [MissionInfo("City Store Robery", "misSrCity", 0f, 0f, 0f, MissionType.Store, true, "None", PatrolZone.City)]
    public class CityStores : Mission
    {
        Dictionary<string, Tuple<Vector3, float>> storeClerkSpawns = new Dictionary<string, Tuple<Vector3, float>>()
        {
            { "city1", new Tuple<Vector3, float>(new Vector3(376.4324f, 321.6145f, 102.4308f), 172.1828f) },
            { "city2", new Tuple<Vector3, float>(new Vector3(-53.92025f, -1757.879f, 28.161f), 131.7822f) },
            { "city3", new Tuple<Vector3, float>(new Vector3(-712.5158f, -917.6412f, 18.21438f), 190.433f) },
            { "city4", new Tuple<Vector3, float>(new Vector3(1159.815f, -327.3766f, 68.20947f), 179.9835f) },
            { "city5", new Tuple<Vector3, float>(new Vector3(1142.246f, -980.7135f, 45.20635f), 269.9512f) },
            { "city6", new Tuple<Vector3, float>(new Vector3(-1491.585f, -384.2828f, 39.0344f), 121.8301f) },
            { "city7", new Tuple<Vector3, float>(new Vector3(-1226.831f, -901.288f, 11.29286f), 35.64202f) },
            { "city8", new Tuple<Vector3, float>(new Vector3(29.44036f, -1349.905f, 28.33013f), 179.9829f) },
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

                RegisteredPeds.Add(storeClerk);

                Notify.DispatchAI("Report of Shoplifting", "Report of a shiplifter, please goto location to gain information.");

                DiscordStatus("Traveling to the scene");

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

                    locationBlip = Functions.SetupLocationBlip(thief.Position.Around(10f, 20f));
                    RegisterBlip(locationBlip);

                    RegisteredPeds.Add(thief);

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

                        Blip b = thief.AttachBlip();
                        b.Sprite = BlipSprite.Enemy;
                        b.Color = BlipColor.Red;
                        b.Scale = 0.5f;
                        b.Priority = 9;

                        missionState = MissionState.SuspectFlee;
                    }

                    break;
            }

            if (thief != null && thief.IsDead)
                Fail("Suspect is dead");

            if (NumberPedsArrested > 0)
                Pass();
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
