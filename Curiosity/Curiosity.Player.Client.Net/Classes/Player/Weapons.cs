using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;

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
                Game.PlayerPed.Weapons.RemoveAll();

            await Task.FromResult(0);
        }
    }
}
