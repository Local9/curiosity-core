using CitizenFX.Core;
using System;
using System.Collections.Generic;

namespace Curiosity.Server.net.Classes.Player
{
    class Sit
    {
        static Server server = Server.GetInstance();
        static List<string> SeatsTaken = new List<string>();

        public static void Init()
        {
            server.RegisterEventHandler("curiosity:Server:Environment:CanTakeSeat", new Action<CitizenFX.Core.Player, string>(CanTakeSeat));
            server.RegisterEventHandler("curiosity:Server:Environment:TakeSeat", new Action<CitizenFX.Core.Player, string>(TakeSeat));
            server.RegisterEventHandler("curiosity:Server:Environment:LeaveSeat", new Action<CitizenFX.Core.Player, string>(LeaveSeat));
        }

        static void CanTakeSeat([FromSource]CitizenFX.Core.Player player, string seatId)
        {
            player.TriggerEvent("curiosity:Player:Environment:CanTakeSeat", seatId, SeatsTaken.Contains(seatId));
        }

        static void TakeSeat([FromSource]CitizenFX.Core.Player player, string seatId)
        {
            lock (SeatsTaken)
            {
                SeatsTaken.Add(seatId);
            }
        }

        static void LeaveSeat([FromSource]CitizenFX.Core.Player player, string seatId)
        {
            lock (SeatsTaken)
            {
                SeatsTaken.Remove(seatId);
            }
        }
    }
}
