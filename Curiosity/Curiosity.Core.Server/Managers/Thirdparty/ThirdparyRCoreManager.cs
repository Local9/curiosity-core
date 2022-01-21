using CitizenFX.Core;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using System;

namespace Curiosity.Core.Server.Managers.Thirdparty
{
    public class ThirdparyRCoreManager : Manager<ThirdparyRCoreManager>
    {
        public override void Begin()
        {
            Instance.EventRegistry.Add("rcore_races:giveMoney", new Action<string, string, CallbackDelegate>((source, amt, cb) =>
            {
                Logger.Info($"rcore_races:giveMoney: PSID: {source} / AMT: {amt}");
                cb.Invoke(true);
            }));

            Instance.EventRegistry.Add("rcore_races:takeMoney", new Action<string, string, CallbackDelegate>((source, amt, cb) =>
            {
                Logger.Info($"rcore_races:takeMoney: PSID: {source} / AMT: {amt}");
                cb.Invoke(true);
            }));

            Instance.EventRegistry.Add("rcore_races:getPlayerId", new Action<string, CallbackDelegate>((sourceStr, cb) =>
            {
                int source = -1;
                if (int.TryParse(sourceStr, out source))
                {
                    if (!PluginManager.ActiveUsers.ContainsKey(source)) return;
                    CuriosityUser user = PluginManager.ActiveUsers[source];
                    Logger.Info($"Player {user.LatestName} has joined a race.");
                    cb.Invoke($"discord:{user.DiscordId}");
                }
                else
                {
                    Logger.Error($"rcore_races:getPlayerId: {sourceStr} invoked");
                }
            }));

            Instance.EventRegistry.Add("rcore_races:getPlayerJob", new Action<string, CallbackDelegate>((source, cb) =>
            {
                Logger.Info($"rcore_races:getPlayerJob invoked");
                cb.Invoke($"homeless");
            }));

            Instance.EventRegistry.Add("rcore_races:showNotification", new Action<string>((msg) =>
            {
                Logger.Info($"rcore_races:notification: {msg}");
                EventSystem.SendAll("ui:notification", eNotification.NOTIFICATION_INFO, msg, "bottom-right", "snackbar", true);
            }));
        }
    }
}
