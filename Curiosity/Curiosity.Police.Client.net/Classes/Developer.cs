using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;

namespace Curiosity.Police.Client.net.Classes
{
    class Developer
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            API.RegisterCommand("co", new Action<int, List<object>, string>(CallOut), false);
        }

        static async void CallOut(int playerHandle, List<object> arguments, string raw)
        {
            if (!Player.PlayerInformation.IsDeveloper()) return;
            await BaseScript.Delay(0);
        }
    }
}
