﻿using CitizenFX.Core;
using System;
using System.Collections.Generic;
using GlobalEntities = Curiosity.Global.Shared.net.Entity;
using GlobalEnums = Curiosity.Global.Shared.net.Enums;
using Curiosity.Shared.Server.net.Helpers;

namespace Curiosity.Server.net.Classes.Menu
{
    class Lists
    {
        static Server server = Server.GetInstance();

        public static void Init()
        {
            server.RegisterEventHandler("curiosity:Server:Menu:Reasons", new Action<CitizenFX.Core.Player, int>(GetReasons));
        }

        static async void GetReasons([FromSource]CitizenFX.Core.Player player, int logGroupId)
        {
            try
            {
                GlobalEnums.LogGroup logGroup = (GlobalEnums.LogGroup)logGroupId;

                if (!Server.isLive)
                {
                    Log.Verbose($"Player {player.Name} has requested {logGroup} reasons.");
                }
                List<GlobalEntities.LogType> logTypes = await Database.DatabaseLog.GetLogReasons(logGroup);
                string reasons = Newtonsoft.Json.JsonConvert.SerializeObject(logTypes);
                player.TriggerEvent($"curiosity:Client:Menu:{logGroup}", reasons);
            }
            catch (Exception ex)
            {
                Log.Error($"GetReasons() -> {ex.Message}");
            }
        }

    }
}
