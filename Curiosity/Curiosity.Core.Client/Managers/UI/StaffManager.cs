using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.PDA;
using Curiosity.Systems.Library.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers
{
    public class StaffManager : Manager<StaffManager>
    {
        public override void Begin()
        {
            Instance.AttachNuiHandler("BanReasons", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin) return null;

                List<LogItem> lst = await EventSystem.Request<List<LogItem>>("user:report:list");

                string jsn = new JsonBuilder().Add("operation", "REPORT_REASONS")
                .Add("list", lst)
                .Build();

                API.SendNuiMessage(jsn);

                return null;
            }));

            Instance.AttachNuiHandler("BanPlayer", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin) return null;

                List<LogItem> lst = await EventSystem.Request<List<LogItem>>("user:report:list");

                string jsn = new JsonBuilder().Add("operation", "REPORT_REASONS")
                .Add("list", lst)
                .Build();

                API.SendNuiMessage(jsn);

                return null;
            }));

            Instance.AttachNuiHandler("KickReasons", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin) return null;

                List<LogItem> lst = await EventSystem.Request<List<LogItem>>("user:report:list");

                string jsn = new JsonBuilder().Add("operation", "REPORT_REASONS")
                .Add("list", lst)
                .Build();

                API.SendNuiMessage(jsn);

                return null;
            }));

            Instance.AttachNuiHandler("KickPlayer", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin) return null;

                List<LogItem> lst = await EventSystem.Request<List<LogItem>>("user:report:list");

                string jsn = new JsonBuilder().Add("operation", "REPORT_REASONS")
                .Add("list", lst)
                .Build();

                API.SendNuiMessage(jsn);

                return null;
            }));
        }
    }
}
