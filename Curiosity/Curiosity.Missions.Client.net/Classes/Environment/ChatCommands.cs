using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static CitizenFX.Core.Native.API;
using CitizenFX.Core;

namespace Curiosity.Missions.Client.net.Classes.Environment
{
    class ChatCommands
    {
        static Client client = Client.GetInstance();


        static public void Init()
        {
            client.RegisterEventHandler("onClientResourceStop", new Action<string>(OnClientResourceStop));
            client.RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));

            RegisterCommand("tcs", new Action<int, List<object>, string>(OnTestCallout), false);
        }

        static void OnClientResourceStop(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;
        }

        static void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;
        }

        static void OnTestCallout(int playerHandle, List<object> arguments, string raw)
        {
            
        }

        static async void Callout()
        {
            try
            {

                await Client.Delay(0);

                // start callout
                Vector3 location = new Vector3(1662.625f, -27.41396f, 173.7747f);
                ClearAreaOfEverything(location.X, location.Y, location.Z, 250f, true, true, true, true);

                Ped painterPed = await Rage.Classes.PedCreator.CreatePedAtLocation(PedHash.Factory01SFY, new Vector3(1657.75f, -56.95895f, 167.1685f), 252.3565f);
                painterPed.Task.PlayAnimation("missheist_agency2aig_4", "look_plan_a_worker1", 3f, -1, AnimationFlags.Loop);

                Ped pedChatter1 = await Rage.Classes.PedCreator.CreatePedAtLocation(PedHash.Factory01SFY, new Vector3(1660.557f, -42.35529f, 168.3279f), 250f);
                pedChatter1.Task.PlayAnimation("missheist_agency2aig_3", "chat_b_worker1", 3f, -1, AnimationFlags.Loop);

                Ped pedChatter2 = await Rage.Classes.PedCreator.CreatePedAtLocation(PedHash.Factory01SMY, new Vector3(1659.94f, -44.33472f, 168.3291f), 275f);
                pedChatter2.Task.PlayAnimation("missheist_agency2aig_3", "chat_b_worker2", 3f, -1, AnimationFlags.Loop);

                Ped pedPatrol = await Rage.Classes.PedCreator.CreatePedAtLocation(PedHash.Lost01GMY, new Vector3(1673.406f, -48.60476f, 173.7747f), 6.851492f);
                Ped pedPatrol2 = await Rage.Classes.PedCreator.CreatePedAtLocation(PedHash.Lost02GMY, new Vector3(1671.406f, -48.60476f, 173.7747f), 6.851492f);

                pedPatrol.Weapons.Give(WeaponHash.AssaultRifle, 1, true, true);
                pedPatrol2.Weapons.Give(WeaponHash.AssaultRifle, 1, true, true);

                List<Vector3> waypoints = new List<Vector3>()
                {
                    new Vector3(1670.834f, -25.44239f, 173.7747f),
                    new Vector3(1662.12f, 2.022691f, 173.7747f),
                    new Vector3(1641.186f, 22.5886f, 173.7743f),
                };

                // pedPatrol2.PedGroup.Add(pedPatrol, true);

                MissionPeds.MissionPed mp = Scripts.MissionPedCreator.Ped(pedPatrol);
                mp.Waypoints = waypoints;
                MissionPeds.MissionPed mp2 = Scripts.MissionPedCreator.Ped(pedPatrol2);
                mp2.Waypoints = waypoints;

                pedPatrol.Task.FightAgainstHatedTargets(100f);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
        }
    }
}
