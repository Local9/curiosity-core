using CitizenFX.Core;
using Curiosity.Missions.Client.net.DataClasses.Mission;
using Curiosity.Missions.Client.net.MissionPeds;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Helper;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

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

        static Blip LocationBlip;

        static public async void Create(Store store)
        {
            CleanUp();

            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "459S Burglar alarm, silent", $"{store.Name}", string.Empty, 2);
            PlaySoundFrontend(-1, "Menu_Accept", "Phone_SoundSet_Default", true);

            MissionPedData1 = store.missionPeds[0];
            MissionPedData2 = store.missionPeds[1];
            MissionPedData3 = store.missionPeds[2];
            MissionPedData4 = store.missionPeds[3];

            MissionPed1 = await CreatePed(MissionPedData1);
            MissionPed2 = await CreatePed(MissionPedData2);

            SetupLocationBlip(store.Location);

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

        static void CleanUp()
        {
            if (LocationBlip != null)
            {
                if (LocationBlip.Exists())
                {
                    LocationBlip.Delete();
                }
            }
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

        static void SetupLocationBlip(Vector3 location)
        {
            LocationBlip = new Blip(AddBlipForCoord(location.X, location.Y, location.Z));
            LocationBlip.Sprite = BlipSprite.BigCircle;
            LocationBlip.Scale = 0.5f;
            LocationBlip.Color = (BlipColor)5;
            LocationBlip.Alpha = 126;
            LocationBlip.ShowRoute = true;
            LocationBlip.Priority = 9;
            LocationBlip.IsShortRange = true;

            SetBlipDisplay(LocationBlip.Handle, 5);
        }
    }
}
