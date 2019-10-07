using CitizenFX.Core;
using Curiosity.Missions.Client.net.Classes.Environment.Rage.Classes;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Helper;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Missions.Client.net.MissionScripts.Police.City.Store
{
    class DavisAve
    {
        static Client client = Client.GetInstance();

        // This can be dumped somewhere...

        static private DataClasses.Mission.Store _store;

        //static string name = "LTD Garage, Davis Ave";

        //static Vector3 location = new Vector3(-53.7861f, -1757.661f, 29.43897f);

        //static Vector3 locationThief1 = new Vector3(-47.62278f, -1753.076f, 29.42101f);
        //static float headingThief1 = 190.1924f;
        //static PedHash modelThief1 = PedHash.ChiGoon01GMM;
        //static WeaponHash weaponThief1 = WeaponHash.Pistol;

        //static Vector3 locationThief2 = new Vector3(-52.41798f, -1748.562f, 29.42101f);
        //static float headingThief2 = 202.6338f;
        //static PedHash modelThief2 = PedHash.ChiGoon01GMM;
        //static WeaponHash weaponThief2 = WeaponHash.MicroSMG;

        //static Vector3 locationThief3 = new Vector3(-43.02503f, -1749.319f, 29.42101f);
        //static float headingThief3 = 29.03807f;
        //static PedHash modelThief3 = PedHash.ChiGoon02GMM;
        //static WeaponHash weaponThief3 = WeaponHash.MicroSMG;

        //static Vector3 locationThief4 = new Vector3(-38.14207f, -1747.355f, 29.19765f);
        //static float headingThief4 = 111.3776f;
        //static PedHash modelThief4 = PedHash.ChiGoon02GMM;
        //static WeaponHash weaponThief4 = WeaponHash.CarbineRifle;

        // end

        static bool createdBackupPed = false;
        static bool createdBackupPed2 = false;

        static MissionPeds.MissionPed missionThief1;
        static MissionPeds.MissionPed missionThief2;

        static public bool Init(DataClasses.Mission.Store store)
        {
            _store = store;

            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "459S Burglar alarm, silent", $"{_store.Name}", string.Empty, 2);
            PlaySoundFrontend(-1, "Menu_Accept", "Phone_SoundSet_Default", true);

            // Clear the area before the player turns up
            ClearAreaOfEverything(_store.Location.X, _store.Location.Y, _store.Location.Z, 50f, true, true, true, true);

            if (PedCreator.PedList.Count > 0)
                EndMission();

            CreateMission();

            // INVOKE
            return true;
        }

        static public bool EndMission()
        {
            while (PedCreator.PedList.Count > 0)
            {
                Ped ped = PedCreator.PedList[0];

                if (NativeWrappers.DoesEntityExist(ped))
                {
                    ped.MarkAsNoLongerNeeded();
                    ped.Delete();
                }

                PedCreator.PedList.Remove(ped);
            }
            return true;
        }

        static async void CreateMission()
        {
            createdBackupPed = false;
            createdBackupPed2 = false;

            Ped thief1 = await PedCreator.CreatePedAtLocation(_store.missionPeds[0].Model, _store.missionPeds[0].SpawnPoint, _store.missionPeds[0].SpawnHeading);
            thief1.Weapons.Give(_store.missionPeds[0].Weapon, 1, true, true);

            Ped thief2 = await PedCreator.CreatePedAtLocation(_store.missionPeds[1].Model, _store.missionPeds[1].SpawnPoint, _store.missionPeds[1].SpawnHeading);
            thief2.Weapons.Give(_store.missionPeds[1].Weapon, 1, true, true);

            //Scripts.ZombieCreator.InfectPed(thief1, 1000);
            //Scripts.ZombieCreator.InfectPed(thief2, 1000);

            missionThief1 = Scripts.MissionPedCreator.Ped(thief1);
            missionThief2 = Scripts.MissionPedCreator.Ped(thief2);

            client.RegisterTickHandler(CreateBackupPed);
            client.RegisterTickHandler(CreateBackupPedTwo);
        }

        static async Task CreateBackupPed()
        {
            try
            {
                if (createdBackupPed)
                {
                    client.DeregisterTickHandler(CreateBackupPed);
                    return;
                }

                if (missionThief1.IsDead || missionThief2.IsDead)
                {
                    if (createdBackupPed) return;

                    if (Client.Random.Next(5) == 1)
                    {
                        createdBackupPed = true;

                        Ped ped = await PedCreator.CreatePedAtLocation(_store.missionPeds[2].Model, _store.missionPeds[2].SpawnPoint, _store.missionPeds[2].SpawnHeading);
                        ped.Weapons.Give(_store.missionPeds[2].Weapon, 1, true, true);

                        MissionPeds.MissionPed missionThiefBackdoor = Scripts.MissionPedCreator.Ped(ped, _store.missionPeds[2].Alertness, _store.missionPeds[2].Difficulty, _store.missionPeds[2].VisionDistance);
                    }

                }
            }
            catch (Exception ex)
            {
                client.DeregisterTickHandler(CreateBackupPedTwo);
            }
            await BaseScript.Delay(100);
        }

        static async Task CreateBackupPedTwo()
        {
            try
            {
                if (createdBackupPed2)
                {
                    client.DeregisterTickHandler(CreateBackupPedTwo);
                    return;
                }

                if (missionThief1.IsDead || missionThief2.IsDead)
                {
                    if (createdBackupPed2) return;

                    if (Client.Random.Next(10) == 1)
                    {
                        createdBackupPed2 = true;

                        Ped ped = await PedCreator.CreatePedAtLocation(_store.missionPeds[3].Model, _store.missionPeds[3].SpawnPoint, _store.missionPeds[3].SpawnHeading);
                        ped.Weapons.Give(_store.missionPeds[3].Weapon, 1, true, true);
                        MissionPeds.MissionPed missionThiefBackdoor = Scripts.MissionPedCreator.Ped(ped, _store.missionPeds[3].Alertness, _store.missionPeds[3].Difficulty, _store.missionPeds[3].VisionDistance);
                    }
                }
            }
            catch (Exception ex)
            {
                client.DeregisterTickHandler(CreateBackupPedTwo);
            }
            await BaseScript.Delay(100);
        }
    }
}
