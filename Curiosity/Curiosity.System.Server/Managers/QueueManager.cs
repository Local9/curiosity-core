using System;
using System.Collections.Generic;
using System.Dynamic;
using Curiosity.System.Library.Models;
using Curiosity.System.Server.Diagnostics;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace Curiosity.System.Server.Managers
{
    public class QueueManager : Manager<QueueManager>
    {
        public override void Begin()
        {
            Curiosity.EventRegistry["playerConnecting"] +=
                new Action<Player, string, CallbackDelegate, ExpandoObject>(OnConnect);
        }

        private async void OnConnect([FromSource] Player player, string name, CallbackDelegate kickManager,
            dynamic deferrals)
        {
            var identifiers = API.GetNumPlayerIdentifiers(player.Handle);
            var discord = "";

            for (var i = 0; i < identifiers; i++)
            {
                var identifier = API.GetPlayerIdentifier(player.Handle, i);

                if (identifier.StartsWith("discord:"))
                {
                    discord = identifier;
                }
            }

            if (discord.Length < 1)
            {
                API.CancelEvent();

                return;
            }

            deferrals.defer();
            deferrals.update("(Life V | forums.lifev.net): Getting User Information...");

            var response =
                (await CuriosityPlugin.Instance.RequestHttp(
                     $"RUN A DISCORD CHECK", new JsonBuilder().Build(),
                     new Dictionary<string, string> {["Content-Type"] = "application/json"}) ?? "[]").Trim();

            if (response.Length < 2 || response == "{}" || response == "[]")
            {
                deferrals.done("(Life V | forums.lifev.net): Please connect and be active within our Discord first: discord.lifev.net");

                return;
            }

            var queueInfo = JsonConvert.DeserializeObject<List<Dictionary<object, object>>>(response);
            var found = default(Dictionary<object, object>);

            foreach (var entry in queueInfo)
            {
                if (!string.Equals(entry["discord"].ToString(), discord,
                    StringComparison.CurrentCultureIgnoreCase)) continue;

                found = entry;

                break;
            }

            if (found == null)
            {
                deferrals.done("(Life V | forums.lifev.net): Please connect and be active within our Discord first: discord.lifev.net");

                return;
            }

            deferrals.done();
        }
    }
}