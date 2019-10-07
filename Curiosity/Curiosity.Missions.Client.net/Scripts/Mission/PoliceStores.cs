using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace Curiosity.Missions.Client.net.Scripts.Mission
{
    class PoliceStores
    {
        static Dictionary<int, DataClasses.Mission.Store> stores = new Dictionary<int, DataClasses.Mission.Store>();

        static public void Init()
        {
            List<DataClasses.Mission.MissionPedData> pedData = new List<DataClasses.Mission.MissionPedData>
            {
                new DataClasses.Mission.MissionPedData {  },
            };
            stores.Add(1, new DataClasses.Mission.Store { Name = "Store 1", Location = new Vector3(), missionPeds = pedData });
        }

    }
}
