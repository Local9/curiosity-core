using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.Client.net.Classes.Player
{
    class Weapons
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            // Need to block all guns, unless whitelisted
            client.RegisterTickHandler(OnTick);
        }

        static async Task OnTick()
        {
            if (!PlayerInformation.IsDeveloper())
            {
                Game.PlayerPed.Weapons.RemoveAll();
                API.BlockWeaponWheelThisFrame();
                Game.PlayerPed.Weapons.Select(WeaponHash.Unarmed, true);
            }

            await Task.FromResult(0);
        }
    }
}
