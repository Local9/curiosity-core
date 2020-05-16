using CitizenFX.Core;
using Curiosity.Shared.Server.net.Helpers;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

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
            server.RegisterEventHandler("playerDropped", new Action<CitizenFX.Core.Player, string>(PlayerDropped));
        }

        static void PlayerDropped([FromSource]Player player, string reason)
        {
            try
            {
                ConcurrentDictionary<int, Tuple<string, int>> listToRun = CalloutsActive;
                foreach (var keyValuePair in listToRun)
                {
                    if (keyValuePair.Value.Item1 == player.Handle)
                    {
                        Tuple<string, int> thing;
                        CalloutsActive.TryRemove(keyValuePair.Key, out thing);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"PlayerDropped (REMOVE CALLOUTS) -> {ex.Message}");
            }
        }

        static async void OnCalloutFree([FromSource]Player player, int calloutId, int patrolZone)
        {
            await RemoveCallout(player);

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

        static async void OnCalloutEnded([FromSource]Player player, int calloutId)
        {
            try
            {
                await RemoveCallout(player);
                player.TriggerEvent("curiosity:Client:Police:PlayerCanTakeCallout");
            }
            catch (Exception ex)
            {
                Log.Error($"OnCalloutEnded -> {ex.Message}");
            }
        }

        static async Task RemoveCallout(Player player)
        {
            ConcurrentDictionary<int, Tuple<string, int>> listToRun = CalloutsActive;
            foreach (var keyValuePair in listToRun)
            {
                if (keyValuePair.Value.Item1 == player.Handle)
                {
                    Tuple<string, int> thing;
                    CalloutsActive.TryRemove(keyValuePair.Key, out thing);
                }
            }
            await Task.FromResult(0);
        }
    }
}
