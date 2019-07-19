using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Models;
using GlobalEntity = Curiosity.Global.Shared.net.Entity;

namespace Curiosity.Mobile.Client.net
{
    public class Client : BaseScript
    {
        private static Client _instance;
        public static PlayerList players;

        public static Client GetInstance()
        {
            return _instance;
        }

        public Client()
        {
            _instance = this;

            players = Players;

            ClassLoader.Init();

            Log.Info("Curiosity.Mobile.Client.net loaded\n");
        }
    }
}
