using CitizenFX.Core;
using Curiosity.Shared.Server.net.Helpers;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Global.Shared.net;
using Newtonsoft.Json;

namespace Curiosity.Missions.Server.net.Environment
{
    class MissionManager
    {
        static Server server = Server.GetInstance();

        static ConcurrentDictionary<int, Tuple<string, int>> MissionsActive = new ConcurrentDictionary<int, Tuple<string, int>>();

        public static void Init()
        {
            server.RegisterEventHandler("playerDropped", new Action<Player, string>(OnPlayerDropped));

            server.RegisterEventHandler("curiosity:Server:Missions:Available ", new Action<Player, string>(OnMissionAvailable));
            server.RegisterEventHandler("curiosity:Server:Missions:Ended", new Action<Player, int>(OnMissionEnded));
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

        static async void OnMissionAvailable([FromSource]Player player, string missionMessage)
        {
            await RemovePlayerMission(player);

            MissionCreate missionCreate = JsonConvert.DeserializeObject<MissionCreate>(Encode.Base64ToString(missionMessage));

            if (MissionsActive.ContainsKey(missionCreate.MissionId))
            {
                player.TriggerEvent("curiosity:Client:Mission:NotAvailable");
            }
            else
            {
                MissionsActive.TryAdd(missionCreate.MissionId, new Tuple<string, int>(player.Handle, missionCreate.PatrolZone));
                player.TriggerEvent("curiosity:Client:Mission:Start", missionMessage);
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
