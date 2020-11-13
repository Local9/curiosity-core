using CitizenFX.Core;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.Systems.Library.Enums;
using System;
using System.Collections.Generic;
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

        Ped storeClerk;
        Ped thief;

        public async override void Start()
        {

            MissionManager.Instance.RegisterTickHandler(OnMissionTick);
        }

        public override void End()
        {
            MissionManager.Instance.DeregisterTickHandler(OnMissionTick);
        }

        async Task OnMissionTick()
        {

        }
    }
}
