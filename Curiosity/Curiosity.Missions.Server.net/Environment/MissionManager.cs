using CitizenFX.Core;
using Curiosity.Shared.Server.net.Helpers;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Curiosity.Missions.Server.net.Environment
{
    class MissionManager
    {
        static Server server = Server.GetInstance();

        static ConcurrentDictionary<int, Tuple<string, int>> MissionsActive = new ConcurrentDictionary<int, Tuple<string, int>>();

        public static void Init()
        {
            server.RegisterEventHandler("curiosity:Server:Mission:Available ", new Action<Player, int, int>(OnMissionAvailable));
            server.RegisterEventHandler("curiosity:Server:Mission:Ended", new Action<Player, int>(OnMissionEnded));
        }

        static public void OnPlayerDropped([FromSource]Player player, string reason)
        {
            try
            {
                ConcurrentDictionary<int, Tuple<string, int>> listToRun = MissionsActive;
                foreach (var keyValuePair in listToRun)
                {
                    if (keyValuePair.Value.Item1 == player.Handle)
                    {
                        Tuple<string, int> thing;
                        MissionsActive.TryRemove(keyValuePair.Key, out thing);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[MissionManager] PlayerDropped -> {ex.Message}");
            }
        }

        static async void OnMissionAvailable([FromSource]Player player, int calloutId, int patrolZone)
        {
            await RemovePlayerMission(player);

            if (MissionsActive.ContainsKey(calloutId))
            {
                player.TriggerEvent("curiosity:Client:Mission:NotAvailable");
            }
            else
            {
                MissionsActive.TryAdd(calloutId, new Tuple<string, int>(player.Handle, patrolZone));
                player.TriggerEvent("curiosity:Client:Mission:Start", calloutId, patrolZone);
            }
        }

        static async void OnMissionEnded([FromSource]Player player, int calloutId)
        {
            try
            {
                await RemovePlayerMission(player);
                player.TriggerEvent("curiosity:Client:Mission:PlayerAvailable");
            }
            catch (Exception ex)
            {
                Log.Error($"[MissionManager] OnMissionEnded -> {ex.Message}");
            }
        }

        static async Task RemovePlayerMission(Player player)
        {
            ConcurrentDictionary<int, Tuple<string, int>> listToRun = MissionsActive;
            foreach (var keyValuePair in listToRun)
            {
                if (keyValuePair.Value.Item1 == player.Handle)
                {
                    Tuple<string, int> thing;
                    MissionsActive.TryRemove(keyValuePair.Key, out thing);
                }
            }
            await Task.FromResult(0);
        }
    }
}
