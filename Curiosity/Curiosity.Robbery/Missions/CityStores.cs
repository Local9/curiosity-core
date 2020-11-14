using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
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

                thief = await Ped.SpawnRandom(storeClerkPosition.AroundStreet(200f, 400f), isNetworked: false);

                if (thief == null)
                {
                    Stop(EndState.Error);
                    return;
                }

                storeClerk.IsImportant = true;
                storeClerk.IsMission = true;
                storeClerk.Fx.Task.WanderAround();

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
                        Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CONTEXT~ to talk to the ~b~Store Clerk~w~.");

                        if (Game.IsControlJustPressed(0, Control.Context))
                        {
                            Screen.ShowSubtitle($"The perp has just ran off, he's not far away.");
                            missionState = MissionState.LookingForSuspect;
                        }
                    }

                    break;
                case MissionState.LookingForSuspect:

                    break;
            }
        }
    }

    internal enum MissionState
    {
        Started,
        End,
        SpokenToClerk,
        LookingForSuspect
    }
}
