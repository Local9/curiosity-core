using System;
using System.Collections.Generic;
using Curiosity.Shared.Client.net.Extensions;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.UI;
using CitizenFX.Core;

namespace Curiosity.Missions.Client.net.Classes.Environment
{
    class ChatCommands
    {
        static Client client = Client.GetInstance();

        static public void Init()
        {
            RegisterCommand("callout", new Action<int, List<object>, string>(OnTestCallout), false);
            RegisterCommand("volume", new Action<int, List<object>, string>(OnAudioVolume), false);

            RegisterCommand("create", new Action<int, List<object>, string>(OnCreateCommand), false);
        }

        static async void OnCreateCommand(int playerHandle, List<object> arguments, string raw)
        {
            if (!PlayerClient.ClientInformation.IsDeveloper())
            {
                Screen.ShowNotification("~r~Method Protected");
            }

            if (arguments.Count < 1) return;

            string type = $"{arguments[0]}";

            if (type == "ped")
            {
                bool killPed = false;
                if (arguments.Count == 2)
                {
                    if ($"{arguments[1]}" == "dead")
                        killPed = true;
                }

                Screen.ShowNotification("~g~Creating Interactive Ped");
                Vector3 spawnPosition = await Game.PlayerPed.GetOffsetPosition(new Vector3(0f, 2f, 0f)).Ground();

                Model model = PedHash.Abigail;
                await model.Request(10000);

                Ped ped = await World.CreatePed(model, spawnPosition);

                model.MarkAsNoLongerNeeded();

                if (ped != null)
                {
                    MissionPeds.InteractivePed interactivePed = Scripts.PedCreators.InteractivePedCreator.Ped(ped);
                    
                    if (killPed)
                    {
                        Ped p = interactivePed;
                        p.Kill();
                    }
                }
            }
        }

        static void OnAudioVolume(int playerHandle, List<object> arguments, string raw)
        {
            if (arguments.Count < 1) return;
            float defaultValue = 0.5f;
            float.TryParse($"{arguments[0]}", out defaultValue);
            Scripts.SoundManager.AudioVolume = defaultValue;
            
            Screen.ShowNotification($"Volume Updated: {Scripts.SoundManager.AudioVolume}");
        }

        static void OnTestCallout(int playerHandle, List<object> arguments, string raw)
        {
            if (PlayerClient.ClientInformation.privilege != Global.Shared.net.Enums.Privilege.DEVELOPER)
            {
                Debug.WriteLine("Not Enough Permissions");
                return;
            }

            if (arguments.Count < 1)
            {
                Screen.ShowNotification("Not enough arguements");
                return;
            }

            int mission = int.Parse($"{arguments[0]}");
            int location = int.Parse($"{arguments[1]}");

            Dictionary<int, DataClasses.Mission.Store> missions = null;

            if (location == 1)
            {
                missions = DataClasses.Mission.PoliceStores.storesCity;
            }

            if (location == 2)
            {
                missions = DataClasses.Mission.PoliceStores.storesCountry;
            }

            if (location == 3)
            {
                missions = DataClasses.Mission.PoliceStores.storesRural;
            }

            if (missions == null)
            {
                Screen.ShowNotification("Mission location not set");
                return;
            }

            if (!missions.ContainsKey(mission))
            {
                Screen.ShowNotification("Mission not found");
                return;
            }

            DataClasses.Mission.Store storeMission = missions[mission];

            Screen.ShowNotification($"Mission {storeMission.Name}");

            if (arguments.Count > 2)
            {
                Game.PlayerPed.Position = storeMission.Location;
            }
            else
            {
                Scripts.Mission.CreateStoreMission.Create(storeMission);
            }
        }

        //static async void Callout()
        //{
        //    try
        //    {

        //        await Client.Delay(0);

        //        // start callout
        //        Vector3 location = new Vector3(1662.625f, -27.41396f, 173.7747f);
        //        ClearAreaOfEverything(location.X, location.Y, location.Z, 250f, true, true, true, true);

        //        Ped painterPed = await PedCreator.CreatePedAtLocation(PedHash.Factory01SFY, new Vector3(1657.75f, -56.95895f, 167.1685f), 252.3565f);
        //        painterPed.Task.PlayAnimation("missheist_agency2aig_4", "look_plan_a_worker1", 3f, -1, AnimationFlags.Loop);

        //        Ped pedChatter1 = await PedCreator.CreatePedAtLocation(PedHash.Factory01SFY, new Vector3(1660.557f, -42.35529f, 168.3279f), 250f);
        //        pedChatter1.Task.PlayAnimation("missheist_agency2aig_3", "chat_b_worker1", 3f, -1, AnimationFlags.Loop);

        //        Ped pedChatter2 = await PedCreator.CreatePedAtLocation(PedHash.Factory01SMY, new Vector3(1659.94f, -44.33472f, 168.3291f), 275f);
        //        pedChatter2.Task.PlayAnimation("missheist_agency2aig_3", "chat_b_worker2", 3f, -1, AnimationFlags.Loop);

        //        Ped pedPatrol = await PedCreator.CreatePedAtLocation(PedHash.Lost01GMY, new Vector3(1673.406f, -48.60476f, 173.7747f), 6.851492f);
        //        Ped pedPatrol2 = await PedCreator.CreatePedAtLocation(PedHash.Lost02GMY, new Vector3(1671.406f, -48.60476f, 173.7747f), 6.851492f);

        //        pedPatrol.Weapons.Give(WeaponHash.AssaultRifle, 1, true, true);
        //        pedPatrol2.Weapons.Give(WeaponHash.AssaultRifle, 1, true, true);

        //        List<Vector3> waypoints = new List<Vector3>()
        //        {
        //            new Vector3(1670.834f, -25.44239f, 173.7747f),
        //            new Vector3(1662.12f, 2.022691f, 173.7747f),
        //            new Vector3(1641.186f, 22.5886f, 173.7743f),
        //        };

        //        // pedPatrol2.PedGroup.Add(pedPatrol, true);

        //        MissionPeds.MissionPed mp = Scripts.MissionPedCreator.Ped(pedPatrol);
        //        mp.Waypoints = waypoints;
        //        MissionPeds.MissionPed mp2 = Scripts.MissionPedCreator.Ped(pedPatrol2);
        //        mp2.Waypoints = waypoints;

        //        pedPatrol.Task.FightAgainstHatedTargets(100f);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"{ex}");
        //    }
        //}
    }
}
