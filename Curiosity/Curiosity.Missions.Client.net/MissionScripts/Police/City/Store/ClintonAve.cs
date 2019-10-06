﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

using Curiosity.Missions.Client.net.Classes.Environment.Rage.Classes;
using Curiosity.Shared.Client.net.Helper;

namespace Curiosity.Missions.Client.net.MissionScripts.Police.City.Store
{
    class ClintonAve
    {
        static Client client = Client.GetInstance();

        static string name = "24/7, Clinton Ave";
        static Vector3 location = new Vector3(1662.625f, -27.41396f, 173.7747f);

        static float hateRadius = 75f;
        static bool createdBackupPed = false;
        static bool createdSniperPed = false;

        static MissionPeds.MissionPed missionThief1;
        static MissionPeds.MissionPed missionThief2;

        static public bool Init()
        {
            // Clear the area before the player turns up
            ClearAreaOfEverything(location.X, location.Y, location.Z, 50f, true, true, true, true);

            CreateMission();

            // INVOKE
            return true;
        }

        static async void CreateMission()
        {
            Ped thief1 = await PedCreator.CreatePedAtLocation(PedHash.ChiGoon01GMM, new Vector3(375.6602f, 325.6703f, 103.5664f), 255.8121f);
            thief1.Weapons.Give(WeaponHash.Pistol, 1, true, true);
            thief1.Task.FightAgainstHatedTargets(hateRadius);

            Ped thief2 = await PedCreator.CreatePedAtLocation(PedHash.ChiGoon02GMM, new Vector3(381.166f, 327.2303f, 103.5664f), 109.4753f);
            thief1.Weapons.Give(WeaponHash.Pistol, 1, true, true);
            thief2.Task.FightAgainstHatedTargets(hateRadius);

            missionThief1 = Scripts.MissionPedCreator.Ped(thief1);
            missionThief2 = Scripts.MissionPedCreator.Ped(thief2);

            client.RegisterTickHandler(CreateBackupPed);
            client.RegisterTickHandler(CreateSniperPed);
        }

        static async Task CreateBackupPed()
        {
            if (createdBackupPed)
            {
                client.DeregisterTickHandler(CreateBackupPed);
                return;
            }

            if (!NativeWrappers.IsEntityAlive(missionThief1) || !NativeWrappers.IsEntityAlive(missionThief2))
            {
                if (createdBackupPed) return;

                if (Client.Random.Next(5) == 1)
                {
                    createdBackupPed = true;

                    float _visionDistance = 100f;

                    Ped thiefBackdoor = await PedCreator.CreatePedAtLocation(PedHash.ChiGoon01GMM, new Vector3(381.946f, 358.5238f, 102.5128f), 84.95505f);
                    thiefBackdoor.Weapons.Give(WeaponHash.Pistol, 1, true, true);
                    MissionPeds.MissionPed missionThiefBackdoor = Scripts.MissionPedCreator.Ped(thiefBackdoor, Extensions.Alertness.FullyAlert, Extensions.Difficulty.BringItOn, _visionDistance);
                }

            }

            await BaseScript.Delay(100);
        }

        static async Task CreateSniperPed()
        {
            if (createdSniperPed)
            {
                client.DeregisterTickHandler(CreateSniperPed);
                return;
            }

            if (!NativeWrappers.IsEntityAlive(missionThief1) || !NativeWrappers.IsEntityAlive(missionThief2))
            {
                if (createdSniperPed) return;

                if (Client.Random.Next(10) == 1)
                {
                    createdSniperPed = true;

                    float _visionDistance = 500f;

                    Ped thiefSniper = await PedCreator.CreatePedAtLocation(PedHash.ChiGoon01GMM, new Vector3(365.0694f, 254.3395f, 112.8926f), 355.4359f);
                    thiefSniper.Weapons.Give(WeaponHash.SniperRifle, 1, true, true);
                    MissionPeds.MissionPed missionThiefSniper = Scripts.MissionPedCreator.Ped(thiefSniper, Extensions.Alertness.FullyAlert, Extensions.Difficulty.BringItOn, _visionDistance);
                }
            }
            

            await BaseScript.Delay(100);
        }
    }
}
