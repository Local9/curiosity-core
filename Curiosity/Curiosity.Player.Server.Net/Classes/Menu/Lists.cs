using CitizenFX.Core;
using System;
using System.Collections.Generic;
using GlobalEntities = Curiosity.Global.Shared.net.Entity;
using GlobalEnums = Curiosity.Global.Shared.net.Enums;

namespace Curiosity.Server.net.Classes.Menu
{
    class Lists
    {
        static Server server = Server.GetInstance();

        public static void Init()
        {
            server.RegisterEventHandler("curiosity:Server:Menu:Reasons", new Action<Player, int>(GetReasons));
        }

        static async void GetReasons([FromSource]Player player, int logGroupId)
        {
            GlobalEnums.LogGroup logGroup = (GlobalEnums.LogGroup)logGroupId;
            List<GlobalEntities.LogType> logTypes = await Database.DatabaseLog.GetLogReasons(logGroup);
            string reasons = Newtonsoft.Json.JsonConvert.SerializeObject(logTypes);
            player.TriggerEvent($"curiosity:Client:Menu:{logGroup}", reasons);
        }

    }
}
