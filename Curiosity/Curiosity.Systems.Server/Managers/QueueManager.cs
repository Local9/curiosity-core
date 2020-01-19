using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Diagnostics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Curiosity.Systems.Server.Managers
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
            Logger.Info($"Player {player.Name} is connecting.");

            var discord = player.Identifiers["discord"];
            var discordGuildId = API.GetConvar("discord_guild", "MISSING");

            if (discordGuildId == "MISSING")
            {
                Logger.Error($"convar 'discord_guild' missing");
                API.CancelEvent();
                return;
            }

            if (discord.Length < 1)
            {
                Logger.Info($"Player {player.Name} missing discord");
                API.CancelEvent();
                return;
            }

            deferrals.defer();
            deferrals.update("(Life V | forums.lifev.net): Getting User Information...");

            var response =
                (await CuriosityPlugin.Instance.RequestHttp(
                     $"https://discordapp.com/api/guilds/{discordGuildId}/members/{discord}", new JsonBuilder().Build(),
                     new Dictionary<string, string> {["Content-Type"] = "application/json"}) ?? "[]").Trim();

            Logger.Debug($"Discord Response: {response}");

            var queueInfo = JsonConvert.DeserializeObject<List<Dictionary<object, object>>>(response);
            var found = default(Dictionary<object, object>);

            foreach (var entry in queueInfo)
            {
                Logger.Info($"Player {player.Name} entry {entry}");

                if (!string.Equals(entry["discord"].ToString(), discord,
                    StringComparison.CurrentCultureIgnoreCase)) continue;

                found = entry;

                deferrals.update("(Life V | forums.lifev.net): Thank you for joining our @ discord.lifev.net");
                Logger.Info($"Player {player.Name} was found on our Discord");

                break;
            }

            deferrals.done();
        }
    }
}