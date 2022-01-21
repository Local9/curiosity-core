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
            Instance.EventRegistry.Add("rcore_races:giveMoney", new Action<int, int, CallbackDelegate>((source, amt, cb) =>
            {
                Logger.Info($"rcore_races:giveMoney: PSID: {source} / AMT: {amt}");
                cb.Invoke(true);
            }));

            Instance.EventRegistry.Add("rcore_races:takeMoney", new Action<int, int, CallbackDelegate>((source, amt, cb) =>
            {
                Logger.Info($"rcore_races:takeMoney: PSID: {source} / AMT: {amt}");
                cb.Invoke(true);
            }));

            Instance.EventRegistry.Add("rcore_races:getPlayerId", new Action<int, CallbackDelegate>((source, cb) =>
            {
                if (PluginManager.ActiveUsers.ContainsKey(source)) return;
                CuriosityUser user = PluginManager.ActiveUsers[source];
                Logger.Info($"Player {user.LatestName} has joined a race.");
                cb.Invoke($"discord:{user.DiscordId}");
            }));

            Instance.EventRegistry.Add("rcore_races:showNotification", new Action<string>((msg) =>
            {
                Logger.Info($"rcore_races:notification: {msg}");
                EventSystem.SendAll("ui:notification", eNotification.NOTIFICATION_INFO, msg, "bottom-right", "snackbar", true);
            }));
        }
    }
}
