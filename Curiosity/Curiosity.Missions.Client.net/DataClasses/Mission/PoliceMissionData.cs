using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Missions.Client.Classes.PlayerClient;
using Curiosity.Missions.Client.MissionPeds;
using Curiosity.Missions.Client.Scripts;
using Curiosity.Missions.Client.Scripts.PedCreators;
using Curiosity.Missions.Client.Static;
using Curiosity.Shared.Client.net.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Missions.Client.DataClasses.Mission
{
    class PoliceMissionData
    {
        static List<PedHash> Families = new List<PedHash>() { PedHash.Famca01GMY, PedHash.Famdnf01GMY, PedHash.Famfor01GMY, PedHash.Families01GFY };
        static List<PedHash> Ballas = new List<PedHash>() { PedHash.BallaEast01GMY, PedHash.BallaOrig01GMY, PedHash.Ballas01GFY, PedHash.BallaSout01GMY };
        static List<WeaponHash> Weapons = new List<WeaponHash>() { WeaponHash.Pistol, WeaponHash.SawnOffShotgun, WeaponHash.MiniSMG, WeaponHash.MicroSMG, WeaponHash.CarbineRifle };

        static MissionData CurrentMission;

        // TODO: Spawner Task (No more than 5 peds spawned at any given time)
        // TODO: Gangs

        /*
         * Player vs all, they will always turn on cops together
         * 
         * Lost vs Mex
         * Ballas vs that other gang
         * find other gang types
         * 
         */

        public static void Init()
        {
            API.RegisterCommand("m", new Action<int, List<object>, string>(RunMission), false);
        }

        static void RunMission(int playerHandle, List<object> arguments, string raw)
        {
            if (!ClientInformation.IsDeveloper) return;

            MissionOne();
        }

        static void MissionOne()
        {
            Relationships.SetupRelationShips();

            MissionData missionData = new MissionData();
            missionData.Name = "Chamberlain Hills";
            missionData.AudioStart = $"RESIDENT/DISPATCH_INTRO_0{PluginManager.Random.Next(1, 3)} UNITS_RESPOND/UNITS_RESPOND_CODE_03_0{PluginManager.Random.Next(1, 3)} WE_HAVE/WE_HAVE_0{PluginManager.Random.Next(1, 3)} CRIMES/CRIME_GUNFIRE_0{PluginManager.Random.Next(1, 4)} CONJUNCTIVES/NEAR_01 AREAS/AREA_CHAMBERLAIN_HILLS_01 RESIDENT/OUTRO_0{PluginManager.Random.Next(1, 4)}";
            missionData.Location = new Vector3(-202.2457f, -1566.719f, 41.67303f);

            missionData.ResurectionRange = 100f;
            missionData.SpawnRange = 250f;

            List<MissionPedData> GangFamilies = new List<MissionPedData>
            {
                CreatePedData(Families[PluginManager.Random.Next(Families.Count)], new Vector3(-190.4952f, -1556.933f, 34.95501f), 0f, Weapons[PluginManager.Random.Next(Weapons.Count)], Relationships.FamiliesRelationship),
                CreatePedData(Families[PluginManager.Random.Next(Families.Count)], new Vector3(-158.5557f, -1545.313f, 34.99964f), 0f, Weapons[PluginManager.Random.Next(Weapons.Count)], Relationships.FamiliesRelationship),
                CreatePedData(Families[PluginManager.Random.Next(Families.Count)], new Vector3(-158.5557f, -1545.313f, 34.99964f), 0f, Weapons[PluginManager.Random.Next(Weapons.Count)], Relationships.FamiliesRelationship)
            };

            if (PluginManager.Random.Next(2) == 1)
            {
                GangFamilies.Add(CreatePedData(Families[PluginManager.Random.Next(Families.Count)], new Vector3(-172.3433f, -1540.761f, 35.13003f), 0f, Weapons[PluginManager.Random.Next(Weapons.Count)], Relationships.FamiliesRelationship));
                GangFamilies.Add(CreatePedData(Families[PluginManager.Random.Next(Families.Count)], new Vector3(-194.5631f, -1555.576f, 38.335f), 0f, Weapons[PluginManager.Random.Next(Weapons.Count)], Relationships.FamiliesRelationship));
                GangFamilies.Add(CreatePedData(Families[PluginManager.Random.Next(Families.Count)], new Vector3(-158.5557f, -1545.313f, 34.99964f), 0f, Weapons[PluginManager.Random.Next(Weapons.Count)], Relationships.FamiliesRelationship));
            }
            else
            {
                GangFamilies.Add(CreatePedData(Families[PluginManager.Random.Next(Families.Count)], new Vector3(-176.1628f, -1533.092f, 34.35227f), 0f, Weapons[PluginManager.Random.Next(Weapons.Count)], Relationships.FamiliesRelationship));
                GangFamilies.Add(CreatePedData(Families[PluginManager.Random.Next(Families.Count)], new Vector3(-185.4702f, -1582.057f, 35.20162f), 0f, Weapons[PluginManager.Random.Next(Weapons.Count)], Relationships.FamiliesRelationship));
                GangFamilies.Add(CreatePedData(Families[PluginManager.Random.Next(Families.Count)], new Vector3(-226.4251f, -1578.034f, 34.3596f), 0f, Weapons[PluginManager.Random.Next(Weapons.Count)], Relationships.FamiliesRelationship));
                GangFamilies.Add(CreatePedData(Families[PluginManager.Random.Next(Families.Count)], new Vector3(-216.1098f, -1614.366f, 34.86931f), 0f, Weapons[PluginManager.Random.Next(Weapons.Count)], Relationships.FamiliesRelationship));
            }


            missionData.MissionGangOne = GangFamilies;

            List<MissionPedData> GangBallas = new List<MissionPedData>
            {
                CreatePedData(Ballas[PluginManager.Random.Next(Ballas.Count)], new Vector3(-176.0499f, -1506.66f, 32.73354f), 0f, Weapons[PluginManager.Random.Next(Weapons.Count)], Relationships.BallasRelationship),
                CreatePedData(Ballas[PluginManager.Random.Next(Ballas.Count)], new Vector3(-176.0499f, -1506.66f, 32.73354f), 0f, Weapons[PluginManager.Random.Next(Weapons.Count)], Relationships.BallasRelationship),
                CreatePedData(Ballas[PluginManager.Random.Next(Ballas.Count)], new Vector3(-194.493f, -1635.402f, 33.38757f), 0f, Weapons[PluginManager.Random.Next(Weapons.Count)], Relationships.BallasRelationship),
                CreatePedData(Ballas[PluginManager.Random.Next(Ballas.Count)], new Vector3(-194.493f, -1635.402f, 33.38757f), 0f, Weapons[PluginManager.Random.Next(Weapons.Count)], Relationships.BallasRelationship),
            };

            if (PluginManager.Random.Next(2) == 1)
            {
                GangBallas.Add(CreatePedData(Ballas[PluginManager.Random.Next(Ballas.Count)], new Vector3(-169.2854f, -1600.612f, 34.01611f), 0f, Weapons[PluginManager.Random.Next(Weapons.Count)], Relationships.BallasRelationship));
                GangBallas.Add(CreatePedData(Ballas[PluginManager.Random.Next(Ballas.Count)], new Vector3(-169.2854f, -1600.612f, 34.01611f), 0f, Weapons[PluginManager.Random.Next(Weapons.Count)], Relationships.BallasRelationship));
                GangBallas.Add(CreatePedData(Ballas[PluginManager.Random.Next(Ballas.Count)], new Vector3(-194.493f, -1635.402f, 33.38757f), 0f, Weapons[PluginManager.Random.Next(Weapons.Count)], Relationships.BallasRelationship));
            }
            else
            {
                GangBallas.Add(CreatePedData(Ballas[PluginManager.Random.Next(Ballas.Count)], new Vector3(-176.0499f, -1506.66f, 32.73354f), 0f, Weapons[PluginManager.Random.Next(Weapons.Count)], Relationships.BallasRelationship));
                GangBallas.Add(CreatePedData(Ballas[PluginManager.Random.Next(Ballas.Count)], new Vector3(-194.493f, -1635.402f, 33.38757f), 0f, Weapons[PluginManager.Random.Next(Weapons.Count)], Relationships.BallasRelationship));
                GangBallas.Add(CreatePedData(Ballas[PluginManager.Random.Next(Ballas.Count)], new Vector3(-169.2854f, -1600.612f, 34.01611f), 0f, Weapons[PluginManager.Random.Next(Weapons.Count)], Relationships.BallasRelationship));
            }

            missionData.MissionGangTwo = GangBallas;

            ExecuteMission(missionData);
        }

        private static async void ExecuteMission(MissionData missionData)
        {
            CurrentMission = missionData;
            //Blip blip = CurrentMission.Blip = World.CreateBlip(CurrentMission.Location);
            //blip.Color = (BlipColor)5;
            //blip.Sprite = BlipSprite.BigCircle;

            //blip.Alpha = 126;
            //blip.ShowRoute = true;
            //blip.Priority = 9;
            //blip.IsShortRange = true;

            //API.SetBlipDisplay(blip.Handle, 5);

            SoundManager.PlayAudio(missionData.AudioStart);

            PedGroup gang1 = new PedGroup(API.CreateGroup(0));
            PedGroup gang2 = new PedGroup(API.CreateGroup(0));

            gang1.FormationType = FormationType.Default;

            while (Game.PlayerPed.Position.DistanceTo(CurrentMission.Location) > 250f)
            {
                await PluginManager.Delay(100);
            }

            bool groupLeader1Exists = false, groupLeader2Exists = false;

            CurrentMission.MissionGangOne.ForEach(async (MissionPedData ped) =>
            {
                MissionPed missionPed = await CreatePed(ped.SpawnPoint, ped.SpawnHeading, ped.Model, ped.Weapon, ped.RelationShipGroup, gang1);

                if (!groupLeader1Exists)
                {
                    groupLeader1Exists = !groupLeader1Exists;
                    API.SetPedAsGroupLeader(missionPed.Handle, gang1.Handle);
                }
                else
                {
                    API.SetPedAsGroupMember(missionPed.Handle, gang1.Handle);
                }
            });

            CurrentMission.MissionGangTwo.ForEach(async (MissionPedData ped) =>
            {
                MissionPed missionPed = await CreatePed(ped.SpawnPoint, ped.SpawnHeading, ped.Model, ped.Weapon, ped.RelationShipGroup, gang2);

                if (!groupLeader2Exists)
                {
                    groupLeader2Exists = !groupLeader2Exists;
                    API.SetPedAsGroupLeader(missionPed.Handle, gang1.Handle);
                }
                else
                {
                    API.SetPedAsGroupMember(missionPed.Handle, gang1.Handle);
                }
            });



        }

        private static async Task<MissionPed> CreatePed(Vector3 pos, float heading, Model selectedModel, WeaponHash weapon, RelationshipGroup relationship, PedGroup group)
        {
            //var l = -4302450214485519674L;
            //Console.WriteLine(l.ToString(("X")));

            API.RequestCollisionAtCoord(pos.X, pos.Y, pos.Z);

            Ped spawnedPed = await World.CreatePed(selectedModel, pos, heading);
            // settings
            spawnedPed.Armor = 100;

            spawnedPed.Weapons.Give(weapon, 999, true, true);
            spawnedPed.DropsWeaponsOnDeath = false;
            spawnedPed.IsOnlyDamagedByPlayer = true;

            API.SetPedRandomProps(spawnedPed.Handle);

            spawnedPed.SetConfigFlag(46, true);

            MissionPed missionPed = MissionPedCreator.Ped(spawnedPed, relationship, Extensions.Alertness.FullyAlert, Extensions.Difficulty.HurtMePlenty);
            selectedModel.MarkAsNoLongerNeeded();

            return missionPed;
        }

        static MissionPedData CreatePedData(PedHash pedHash, Vector3 spawnPos, float heading, WeaponHash weaponHash, RelationshipGroup relationship)
        {
            return new MissionPedData
            {
                Model = pedHash,
                SpawnHeading = heading,
                SpawnPoint = spawnPos,
                Alertness = Extensions.Alertness.FullyAlert,
                Difficulty = Extensions.Difficulty.BringItOn,
                Weapon = weaponHash,
                VisionDistance = 500f,
                RelationShipGroup = relationship
            };
        }
    }
}
