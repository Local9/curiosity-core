﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Shared.Client.net.Extensions;

namespace Curiosity.Police.Client.net.Environment.Vehicle
{
    class LoadoutPosition
    {
        static Client client = Client.GetInstance();

        static public void Init()
        {
            client.DeregisterTickHandler(OnVehicleBootTask);
            client.RegisterTickHandler(OnVehicleBootTask);
        }
        static public void Dispose()
        {
            client.DeregisterTickHandler(OnVehicleBootTask);
        }

        static async Task OnVehicleBootTask()
        {
            await Task.FromResult(0);

            if (Client.CurrentVehicle == null) return;

            EntityBone entityBone = Client.CurrentVehicle.Bones["boot"];
            if (entityBone.Position.Distance(Game.PlayerPed.Position) < 1.2)
            {
                Screen.DisplayHelpTextThisFrame("Press ~INPUT_PICKUP~ to ~b~rearm.");

                if (Game.IsControlJustPressed(0, Control.Pickup))
                {
                    Classes.Menus.MenuLoadout.OnWeaponResupply();
                    await Client.Delay(10000);
                }
            }
        }
    }
}
