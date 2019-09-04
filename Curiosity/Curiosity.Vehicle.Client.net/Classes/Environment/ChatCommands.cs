﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Vehicle.Client.net.Classes.Environment
{
    class ChatCommands
    {
        static Client client = Client.GetInstance();

        static public void Init()
        {
            RegisterCommand("sirens", new Action<int, List<object>, string>(SirensCommand), false);
        }


        static void SirensCommand(int playerHandle, List<object> arguments, string raw)
        {
            ShowSirenKeys();
        }

        static public void ShowSirenKeys()
        {
            Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Sirens", "How To...", "Enable Sirens: ~b~G~n~~s~Change Siren: ~b~Y~n~~s~Horn: ~b~CTRL~n~~s~Blip Siren: ~b~B", 2);
        }
    }
}
