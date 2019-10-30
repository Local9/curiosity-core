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

namespace Curiosity.Missions.Client.net.Scripts
{
    class MissionEvents
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Mission:Start", new Action<string>(OnPlayerCanStartMission));
        }

        static void OnPlayerCanStartMission(string missionData)
        {
            MissionCreate missionMessage = JsonConvert.DeserializeObject<MissionCreate>(Encode.Base64ToString(missionData));

            if (ClientInformation.IsDeveloper())
            {
                Log.Info($"Mission Message {missionMessage}");
            }

            PatrolZone missionPatrolZone = (PatrolZone)missionMessage.PatrolZone;

            Dictionary<int, DataClasses.Mission.Store> missions = new Dictionary<int, DataClasses.Mission.Store>();

            switch (missionPatrolZone)
            {
                case PatrolZone.Rural:
                    missions = DataClasses.Mission.PoliceStores.storesRural;
                    break;
                case PatrolZone.Country:
                    missions = DataClasses.Mission.PoliceStores.storesCountry;
                    break;
                default: // CITY
                    missions = DataClasses.Mission.PoliceStores.storesCity;
                    break;
            }

            DataClasses.Mission.Store mission = missions[missionMessage.MissionId];

            Mission.RandomMissionHandler.SetIsOnActiveCallout(true);

            Mission.CreateStoreMission.Create(mission);
        }
    }
}
