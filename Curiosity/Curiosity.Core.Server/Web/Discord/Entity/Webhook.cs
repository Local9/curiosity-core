﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Web.Discord.Entity
{
    class Webhook
    {
        static Request request = new Request();

        public string Url;

        [JsonProperty(PropertyName = "avatar_url")]
        public string AvatarUrl;

        [JsonProperty(PropertyName = "content")]
        public string Content;

        [JsonProperty(PropertyName = "username")]
        public string Username;

        [JsonProperty(PropertyName = "embeds")]
        public List<Embed> Embeds = new List<Embed>();

        public Webhook(string uri)
        {
            Url = uri;
        }

        public async Task Send()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");

            string discordBotKey = PluginManager.DiscordBotKey;

            headers.Add("Authorization", $"Bot {discordBotKey}");

            string jsonData = JsonConvert.SerializeObject(this);

            await request.Http($"{Url}", "POST", jsonData, headers);
        }
    }
}
