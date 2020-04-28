﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Police.Client.net.Classes.Player;
using Curiosity.Shared.Client.net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    API.SetPedToRagdoll(shark.Handle, 0, 0, 0, true, true, true);
                    shark.CanRagdoll = true;
                    shark.Health = 0;

                    shark.ApplyForce(Game.PlayerPed.ForwardVector * 500);
                }
            }
        }
    }
}
