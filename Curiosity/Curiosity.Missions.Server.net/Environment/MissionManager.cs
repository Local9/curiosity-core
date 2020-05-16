using CitizenFX.Core;
using Curiosity.Global.Shared.net;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Shared.Server.net.Helpers;
using Newtonsoft.Json;
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
            server.RegisterEventHandler("curiosity:Server:Missions:Available", new Action<Player, string>(OnMissionAvailable));
            server.RegisterEventHandler("curiosity:Server:Missions:Ended", new Action<Player, int>(OnMissionEnded));
            server.RegisterEventHandler("curiosity:Server:Missions:Dispatch", new Action<Player>(OnDispatch));
        }

        static void OnDispatch([FromSource]Player player)
        {
            string json = JsonConvert.SerializeObject(MissionsActive);

            string encoded = Encode.StringToBase64(json);

            player.TriggerEvent("curiosity:Client:Mission:Dispatch", encoded);
        }

        static public async void OnPlayerDropped([FromSource]Player player, string reason)
        {
            try
            {
                await RemovePlayerMission(player);
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

            Log.Info($"{player.Name}: MissionId[{missionCreate.MissionId}], Zone[{missionCreate.PatrolZone}]");

            if (MissionsActive.ContainsKey(missionCreate.MissionId))
            {
                player.TriggerEvent("curiosity:Client:Mission:NotAvailable");
            }
            else
            {
                MissionsActive.TryAdd(missionCreate.MissionId, new Tuple<string, int>(player.Handle, missionCreate.PatrolZone));

                string json = JsonConvert.SerializeObject(missionCreate);

                string encoded = Encode.StringToBase64(json);

                player.TriggerEvent("curiosity:Client:Mission:Start", encoded);
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
            if (!Server.isLive)
            {
                Log.Info($"[RemovePlayerMission] Start {player.Name}");
            }

            ConcurrentDictionary<int, Tuple<string, int>> listToRun = MissionsActive;
            foreach (var keyValuePair in listToRun)
            {
                if (keyValuePair.Value.Item1 == player.Handle)
                {
                    Tuple<string, int> thing;
                    MissionsActive.TryRemove(keyValuePair.Key, out thing);

                    if (!Server.isLive)
                    {
                        Log.Info($"[RemovePlayerMission] Mission found and removed.");
                    }
                }
            }

            if (!Server.isLive)
            {
                Log.Info($"[RemovePlayerMission] Mission Cache Count {MissionsActive.Count}");
            }

            await Task.FromResult(0);
        }
    }
}
