using System;
using System.Collections.Concurrent;
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

        static ConcurrentDictionary<int, Tuple<string, int>> CalloutsActive = new ConcurrentDictionary<int, Tuple<string, int>>();

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
                CalloutsActive.TryAdd(calloutId, new Tuple<string, int>(player.Handle, patrolZone));
                player.TriggerEvent("curiosity:Client:Police:CalloutStart", calloutId, patrolZone);
            }
        }

        static void OnCalloutEnded([FromSource]Player player, int calloutId)
        {
            try
            {
                ConcurrentDictionary<int, Tuple<string, int>> listToRun = CalloutsActive;
                foreach (var keyValuePair in listToRun)
                {
                    if (keyValuePair.Value.Item1 == player.Handle)
                    {
                        Tuple<string, int> thing;
                        CalloutsActive.TryRemove(calloutId, out thing);
                    }
                }
                player.TriggerEvent("curiosity:Client:Police:CalloutEnded");
            }
            catch (Exception ex)
            {
                Log.Error($"OnCalloutEnded -> {ex.Message}");
            }
        }
    }
}
