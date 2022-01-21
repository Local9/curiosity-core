﻿using CitizenFX.Core;
using Curiosity.Core.Server.Web;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using System;

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

                DiscordClient.SendDiscordPlayerLogMessage($"RACE: Player {user.LatestName} just won a race and earned ${amt:C0}");

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

                DiscordClient.SendDiscordPlayerLogMessage($"RACE: Player {user.LatestName} just entered a race for ${amt:C0}");

                // Player player = PluginManager.PlayersList[source];
                // player.State.Set(StateBagKey.PLAYER_RACE_ACTIVE, false, true);

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
