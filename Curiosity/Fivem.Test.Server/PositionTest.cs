using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fivem.Test.Server
{
    public class PositionTest : BaseScript
    {
        PlayerList players;

        public PositionTest()
        {
            players = Players;
            API.RegisterCommand("coords", new Action<int, List<object>, string>(OnCoordsCommand), false);
        }

        private void OnCoordsCommand(int playerHandle, List<object> args, string raw)
        {
            Player player = players[playerHandle];
            Debug.WriteLine($"Player Ped Coords: {player.Character.Position}");
            Ped copiedPed = new Ped(player.Character.Handle);
            Debug.WriteLine($"Player Ped Copied Coords: {copiedPed.Position}");
        }
    }
}
