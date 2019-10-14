using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CitizenFX.Core;
using System.Threading.Tasks;
using Curiosity.Missions.Client.net.MissionPeds;
using Curiosity.Missions.Client.net.DataClasses.Mission;
using Curiosity.Shared.Client.net.Helper;

namespace Curiosity.Missions.Client.net.Scripts.Mission
{
    class CreateStoreMission
    {
        static Client client = Client.GetInstance();

        static MissionPedData MissionPedData1;
        static MissionPedData MissionPedData2;
        static MissionPedData MissionPedData3;
        static MissionPedData MissionPedData4;

        static MissionPed MissionPed1;
        static MissionPed MissionPed2;

        static public async void Create(Store store)
        {
            MissionPedData1 = store.missionPeds[0];
            MissionPedData2 = store.missionPeds[1];
            MissionPedData3 = store.missionPeds[2];
            MissionPedData4 = store.missionPeds[3];

            MissionPed1 = await CreatePed(MissionPedData1);
            MissionPed2 = await CreatePed(MissionPedData2);

            client.RegisterTickHandler(SpawnBackupPedOne);
            client.RegisterTickHandler(SpawnBackupPedTwo);

            client.RegisterTickHandler(MissionCompleted);
        }

        static async Task SpawnBackupPedOne()
        {
            await Task.FromResult(0);

            if (AreMissionPedsDead() && Client.Random.Next(3) == 1)
            {
                MissionPed ped = await CreatePed(MissionPedData3);
            }
        }

        static async Task SpawnBackupPedTwo()
        {
            await Task.FromResult(0);

            if (AreMissionPedsDead() && Client.Random.Next(5) == 1)
            {
                MissionPed ped = await CreatePed(MissionPedData4);
            }
        }

        static async Task MissionCompleted()
        {
            await Task.FromResult(0);
        }

        static async Task<MissionPed> CreatePed(MissionPedData missionPedData)
        {
            Ped backup = await PedCreator.CreatePedAtLocation(missionPedData.Model, missionPedData.SpawnPoint, missionPedData.SpawnHeading);
            return MissionPedCreator.Ped(backup, missionPedData.Alertness, missionPedData.Difficulty, missionPedData.VisionDistance);
        }

        static bool AreMissionPedsDead()
        {
            return (!NativeWrappers.IsEntityAlive(MissionPed1) || NativeWrappers.IsEntityAlive(MissionPed2));
        }
    }
}
