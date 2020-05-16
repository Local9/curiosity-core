using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Police.Client.net.Classes.Player;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Police.Client.net.Environment.WeaponScripts
{
    class SharkLauncher
    {
        static Client client = Client.GetInstance();
        static Random random = new Random();
        static bool SharkLauncherEnabled = false;

        public static void Init()
        {
            API.RegisterCommand("shark", new Action<int, List<object>, string>(OnSharkCommand), false);
        }

        private static void OnSharkCommand(int playerHandle, List<object> arguments, string raw)
        {
            if (!PlayerInformation.IsDeveloper()) return;

            SharkLauncherEnabled = !SharkLauncherEnabled;

            if (SharkLauncherEnabled)
            {
                client.RegisterTickHandler(OnSharkTick);
            }
            else
            {
                client.DeregisterTickHandler(OnSharkTick);
            }
        }

        private static async Task OnSharkTick()
        {

            if (!PlayerInformation.IsDeveloper())
            {
                client.DeregisterTickHandler(OnSharkTick);
                return;
            };

            if (Game.PlayerPed.Weapons.Current.Hash == WeaponHash.AssaultRifle)
            {
                if (Game.IsControlJustPressed(0, Control.Attack))
                {
                    Vector3 spawnLocation = Game.PlayerPed.Position + (Game.PlayerPed.ForwardVector * 5);
                    string[] sharkarray = { "a_c_sharktiger", "a_c_sharkhammer" };
                    string deadshark = sharkarray[random.Next(sharkarray.Length + 1)];
                    Ped shark = await World.CreatePed(deadshark, spawnLocation);
                    SharkEntity sharkEntity = new SharkEntity(shark);
                }
            }
        }

        public class SharkEntity
        {
            public Ped Ped;
            long TimeSpawned;

            public SharkEntity(Ped ped)
            {
                Ped = ped;

                API.SetPedToRagdoll(ped.Handle, 0, 0, 0, true, true, true);
                ped.CanRagdoll = true;
                ped.Health = 0;

                ped.ApplyForce(Game.PlayerPed.ForwardVector * 500);

                TimeSpawned = API.GetGameTimer();

                Client.GetInstance().RegisterTickHandler(OnSharkSpawned);
            }

            async Task OnSharkSpawned()
            {
                while ((API.GetGameTimer() - TimeSpawned) < 60000)
                {
                    await Client.Delay(0);
                }
                if (Ped != null)
                {
                    if (Ped.Exists())
                    {
                        Ped.Delete();
                    }
                }
                Client.GetInstance().DeregisterTickHandler(OnSharkSpawned);
            }
        }
    }
}
