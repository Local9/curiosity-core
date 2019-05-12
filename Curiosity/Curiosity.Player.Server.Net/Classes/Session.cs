using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Curiosity.Server.net.Enums;

namespace Curiosity.Server.net.Classes
{
    public class Session
    {
        // Session data
        public string NetID { get; private set; }
        public string Name { get; private set; }
        public string[] Identities { get; private set; }
        // public Models.SteamID SteamID { get; private set; } = null;

        // Player data
        public int UserID { get; set; }
        public Privilege Privilege { get; set; }

        // Player states
        public bool HasSpawned { get; private set; }
        public bool IsLoggedIn { get { return UserID > 0; } private set { } }
        // public bool IsPlaying { get { return Character != null; } private set { } }


    }
}
