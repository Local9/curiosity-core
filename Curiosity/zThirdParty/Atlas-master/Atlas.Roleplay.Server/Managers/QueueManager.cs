using Atlas.Roleplay.Library.Models;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Atlas.Roleplay.Server.Managers
{
    public class QueueManager : Manager<QueueManager>
    {
        public override void Begin()
        {
            Atlas.EventRegistry["playerConnecting"] +=
                new Action<Player, string, CallbackDelegate, ExpandoObject>(OnConnect);
        }

        private async void OnConnect([FromSource] Player player, string name, CallbackDelegate kickManager,
            dynamic deferrals)
        {
            var identifiers = API.GetNumPlayerIdentifiers(player.Handle);
            var steam = "";

            for (var i = 0; i < identifiers; i++)
            {
                var identifier = API.GetPlayerIdentifier(player.Handle, i);

                if (identifier.StartsWith("steam:"))
                {
                    steam = identifier;
                }
            }

            if (steam.Length < 1)
            {
                kickManager.Invoke("(Society | www.societyrp.se): Du måste ha Steam tjänsten igång.");

                API.CancelEvent();

                return;
            }

            deferrals.defer();
            deferrals.update("(Society | www.societyrp.se): Hämtar kö-information...");

            var response =
                (await AtlasPlugin.Instance.RequestHttp(
                     $"https://queue.societyrp.se/service.php?steamId={steam}&action=FETCH", new JsonBuilder().Build(),
                     new Dictionary<string, string> { ["Content-Type"] = "application/json" }) ?? "[]").Trim();

            if (response.Length < 2 || response == "{}" || response == "[]")
            {
                deferrals.done("(Society | www.societyrp.se): Sätt dig i kön först. * queue.societyrp.se");

                return;
            }

            var queueInfo = JsonConvert.DeserializeObject<List<Dictionary<object, object>>>(response);
            var found = default(Dictionary<object, object>);

            foreach (var entry in queueInfo)
            {
                if (!string.Equals(entry["SteamId"].ToString(), steam,
                    StringComparison.CurrentCultureIgnoreCase)) continue;

                found = entry;

                break;
            }

            if (found == null)
            {
                deferrals.done("(Society | www.societyrp.se): Sätt dig i kön först. * queue.societyrp.se");

                return;
            }

            if (int.Parse(found["Place"].ToString()) > 1)
            {
                deferrals.done($"(Society | www.societyrp.se): Det är inte din tur än! ({found["Place"]})");

                return;
            }

            deferrals.done();
        }
    }
}