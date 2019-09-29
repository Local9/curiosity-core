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

        static List<Ped> peds = new List<Ped>();

        static public void Init()
        {
            client.RegisterEventHandler("onClientResourceStop", new Action<string>(OnClientResourceStop));
            client.RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));

            RegisterCommand("tcs", new Action<int, List<object>, string>(OnTestCallout), false);
        }

        static void OnClientResourceStop(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;

            foreach(Ped ped in peds)
            {
                if (ped.Exists())
                {
                    ped.MarkAsNoLongerNeeded();
                    ped.Delete();
                }
            }
        }

        static void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;
            Callout();
        }

        static void OnTestCallout(int playerHandle, List<object> arguments, string raw)
        {
            Callout();
            CitizenFX.Core.UI.Screen.ShowNotification("Called tcs");
        }

        static async void Callout()
        {
            await Client.Delay(0);

            // start callout
            Vector3 location = new Vector3(1662.625f, -27.41396f, 173.7747f);
            ClearAreaOfEverything(location.X, location.Y, location.Z, 250f, true, true, true, true);

            Ped painterPed = await Rage.Classes.PedCreator.CreatePedAtLocation(PedHash.Factory01SFY, new Vector3(1657.75f, -56.95895f, 167.1685f), 252.3565f);
            painterPed.Task.PlayAnimation("missheist_agency2aig_4", "look_plan_a_worker1", 3f, -1, AnimationFlags.Loop);
            peds.Add(painterPed);

            Ped pedChatter1 = await Rage.Classes.PedCreator.CreatePedAtLocation(PedHash.Factory01SFY, new Vector3(1660.557f, -42.35529f, 168.3279f), 250f);
            pedChatter1.Task.PlayAnimation("missheist_agency2aig_3", "chat_b_worker1", 3f, -1, AnimationFlags.Loop);
            peds.Add(pedChatter1);

            Ped pedChatter2 = await Rage.Classes.PedCreator.CreatePedAtLocation(PedHash.Factory01SMY, new Vector3(1659.94f, -44.33472f, 168.3291f), 275f);
            pedChatter2.Task.PlayAnimation("missheist_agency2aig_3", "chat_b_worker2", 3f, -1, AnimationFlags.Loop);
            peds.Add(pedChatter2);
        }
    }
}
