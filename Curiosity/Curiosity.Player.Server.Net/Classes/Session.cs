using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using Curiosity.Server.net.Enums;

namespace Curiosity.Server.net.Classes
{
    public class Session
    {
        const string STEAM_IDENTIFIER = "steam";
        // Session data
        public string NetId { get; private set; }
        public string Name { get; private set; }
        public string[] Identities { get; private set; }
        public string SteamId { get; private set; } = null;

        // Player data
        public int UserID { get; set; }
        public Privilege Privilege { get; set; }

        // Player states
        public bool HasSpawned { get; private set; }
        public bool IsLoggedIn { get { return UserID > 0; } private set { } }
        // public bool IsPlaying { get { return Character != null; } private set { } }

        // Aliases
        public int Ping => GetPlayerPing(NetId);
        public int LastMsg => GetPlayerLastMsg(NetId);
        public string EndPoint => GetPlayerEndpoint(NetId);
        public bool IsDeveloper => Privilege.HasFlag(Privilege.SERVEROWNER);
        public bool IsAdmin => Privilege.HasFlag(Privilege.ADMINISTRATOR);
        public void Drop(string reason) => DropPlayer(NetId, reason);

        public Player Player { get; private set; }
        public SemaphoreSlim Mutex { get; private set; }

        public Session(Player player)
        {
            Mutex = new SemaphoreSlim(1, 1);
            Player = player;
            NetId = player.Handle;

            int numIdents = GetNumPlayerIdentifiers(NetId);
            List<string> idents = new List<string>();

            for (int i = 0; i < numIdents; i++)
            {
                idents.Add(GetPlayerIdentifier(NetId, i));
            }

            Identities = idents.ToArray();

            SteamId = player.Identifiers[STEAM_IDENTIFIER];

            // Set unknowns
            // Character = null;
            UserID = 0;
            Privilege = Privilege.USER;
            HasSpawned = false;
        }
    }
}
