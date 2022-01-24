using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.Core.Server.Web;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using System;
using Newtonsoft.Json;

namespace Curiosity.Core.Server.Managers.Thirdparty
{
    public class ThirdparyRCoreManager : Manager<ThirdparyRCoreManager>
    {
        DiscordClient DiscordClient => DiscordClient.GetModule();

        public override void Begin()
        {
            Instance.EventRegistry.Add("rcore_races:giveMoney", new Action<int, int, CallbackDelegate>((source, amt, cb) =>
            {
                // Logger.Info($"rcore_races:giveMoney: PSID: {source} / AMT: {amt}");
                if (!PluginManager.ActiveUsers.ContainsKey(source)) return;
                CuriosityUser user = PluginManager.ActiveUsers[source];
                Database.Store.BankDatabase.Adjust(user.Character.CharacterId, amt);
                // Player player = PluginManager.PlayersList[source];
                // player.State.Set(StateBagKey.PLAYER_RACE_ACTIVE, false, true);

                string msg = $"RACE: Player {user.LatestName} just earned ${amt:N0} from a race!";
                DiscordClient.SendDiscordPlayerLogMessage(msg);
                EventSystem.SendAll("ui:notification", eNotification.NOTIFICATION_INFO, msg, "bottom-right", "snackbar", true);
                cb.Invoke(true);
            }));

            Instance.EventRegistry.Add("rcore_stickers:payAmount", new Action<int, int, CallbackDelegate>((source, amt, cb) =>
            {
                // Logger.Info($"rcore_races:takeMoney: PSID: {source} / AMT: {amt}");
                if (!PluginManager.ActiveUsers.ContainsKey(source)) return;
                CuriosityUser user = PluginManager.ActiveUsers[source];

                if ((user.Character.Cash - amt) < 0)
                {
                    cb.Invoke(false);
                    return;
                }

                Database.Store.BankDatabase.Adjust(user.Character.CharacterId, amt * -1);
                cb.Invoke(true);
            }));

            Instance.EventRegistry.Add("rcore_races:takeMoney", new Action<int, int, CallbackDelegate>((source, amt, cb) =>
            {
                // Logger.Info($"rcore_races:takeMoney: PSID: {source} / AMT: {amt}");
                if (!PluginManager.ActiveUsers.ContainsKey(source)) return;
                CuriosityUser user = PluginManager.ActiveUsers[source];

                if ((user.Character.Cash - amt) < 0)
                    amt = (int)user.Character.Cash;

                DiscordClient.SendDiscordPlayerLogMessage($"RACE: Player {user.LatestName} just entered a race for ${amt:N0}");

                // Player player = PluginManager.PlayersList[source];
                // player.State.Set(StateBagKey.PLAYER_RACE_ACTIVE, false, true);

                Database.Store.BankDatabase.Adjust(user.Character.CharacterId, amt * -1);
                cb.Invoke(true);
            }));

            Instance.EventRegistry.Add("rcore_stickers:getVehicleInfo", new Action<string, string, CallbackDelegate>((plate, vehicleHash, cb) =>
            {
                var data = new { owner = 1, plate = plate, model = vehicleHash };
                cb.Invoke(JsonConvert.SerializeObject(data));
            }));

            Instance.EventRegistry["rcore_races:getPlayerId"] += new Action<int, CallbackDelegate>(OnGetPlayerIdendifier);
            Instance.EventRegistry["rcore_stickers:getPlayerId"] += new Action<int, CallbackDelegate>(OnGetPlayerIdendifier);

            Instance.EventRegistry["rcore_races:getPlayerJob"] += new Action<int, CallbackDelegate>(OnGetPlayerJob);
            Instance.EventRegistry["rcore_stickers:getPlayerJob"] += new Action<int, CallbackDelegate>(OnGetPlayerJob);

            Instance.EventRegistry["rcore_races:showNotification"] += new Action<string>(OnShowNotification);
            Instance.EventRegistry["rcore_stickers:showNotification"] += new Action<string>(OnShowNotification);
        }

        private void OnGetPlayerJob(int source, CallbackDelegate cb)
        {
            cb.Invoke($"homeless");
        }

        private void OnGetPlayerIdendifier(int source, CallbackDelegate cb)
        {
            if (!PluginManager.ActiveUsers.ContainsKey(source)) return;
            CuriosityUser user = PluginManager.ActiveUsers[source];
            cb.Invoke($"discord:{user.DiscordId}");
        }

        private void OnShowNotification(string msg)
        {
            EventSystem.SendAll("ui:notification", eNotification.NOTIFICATION_INFO, msg, "bottom-right", "snackbar", true);
        }
    }
}
