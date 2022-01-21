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
                // Logger.Info($"rcore_races:giveMoney: PSID: {source} / AMT: {amt}");
                if (!PluginManager.ActiveUsers.ContainsKey(source)) return;
                CuriosityUser user = PluginManager.ActiveUsers[source];
                Database.Store.BankDatabase.Adjust(user.Character.CharacterId, amt);
                cb.Invoke(true);
            }));

            Instance.EventRegistry.Add("rcore_races:takeMoney", new Action<int, int, CallbackDelegate>((source, amt, cb) =>
            {
                // Logger.Info($"rcore_races:takeMoney: PSID: {source} / AMT: {amt}");
                if (!PluginManager.ActiveUsers.ContainsKey(source)) return;
                CuriosityUser user = PluginManager.ActiveUsers[source];

                if ((user.Character.Cash - amt) < 0)
                    amt = (int)user.Character.Cash;

                if (amt == 0)
                {
                    cb.Invoke(false);
                    return;
                }

                Database.Store.BankDatabase.Adjust(user.Character.CharacterId, amt * -1);
                cb.Invoke(true);
            }));

            Instance.EventRegistry.Add("rcore_races:getPlayerId", new Action<int, CallbackDelegate>((source, cb) =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(source)) return;
                CuriosityUser user = PluginManager.ActiveUsers[source];
                cb.Invoke($"discord:{user.DiscordId}");
            }));

            Instance.EventRegistry.Add("rcore_races:getPlayerJob", new Action<string, CallbackDelegate>((source, cb) =>
            {
                // Logger.Info($"rcore_races:getPlayerJob invoked");
                cb.Invoke($"homeless");
            }));

            Instance.EventRegistry.Add("rcore_races:showNotification", new Action<string>((msg) =>
            {
                // Logger.Info($"rcore_races:notification: {msg}");
                EventSystem.SendAll("ui:notification", eNotification.NOTIFICATION_INFO, msg, "bottom-right", "snackbar", true);
            }));
        }
    }
}
