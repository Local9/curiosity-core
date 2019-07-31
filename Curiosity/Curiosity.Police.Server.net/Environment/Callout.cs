using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using Curiosity.Shared.Server.net.Helpers;

namespace Curiosity.Police.Server.net.Environment
{
    class Callout
    {
        static Server server = Server.GetInstance();

        static Dictionary<int, Tuple<string, int>> CalloutsActive = new Dictionary<int, Tuple<string, int>>();

        public static void Init()
        {
            server.RegisterEventHandler("curiosity:Server:Police:CalloutFree", new Action<Player, int, int>(OnCalloutFree));
            server.RegisterEventHandler("curiosity:Server:Police:CalloutEnded", new Action<Player, int>(OnCalloutEnded));
        }

        static void OnCalloutFree([FromSource]Player player, int calloutId, int patrolZone)
        {
            if (CalloutsActive.ContainsKey(calloutId))
            {
                player.TriggerEvent("curiosity:Client:Police:CalloutTaken");
            }
            else
            {
                CalloutsActive.Add(calloutId, new Tuple<string, int>(player.Handle, patrolZone));
                player.TriggerEvent("curiosity:Client:Police:CalloutStart", calloutId, patrolZone);
            }
        }

        static void OnCalloutEnded([FromSource]Player player, int calloutId)
        {
            lock(CalloutsActive)
            {
                CalloutsActive.Remove(calloutId);
                player.TriggerEvent("curiosity:Client:Police:CalloutEnded");
            }
        }
    }
}
