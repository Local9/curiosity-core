﻿namespace Perseverance.Discord.Bot.Entities
{
    public struct Configuration
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("guild")]
        public ulong Guild { get; private set; }

        [JsonProperty("channels")]
        public Dictionary<string, ulong> Channels { get; private set; }

        [JsonProperty("donatorRoles")]
        public Dictionary<string, ulong> DonatorRoles { get; private set; }

        [JsonProperty("database")]
        public DatabaseInstance DatabaseInstance { get; private set; }

        [JsonProperty("servers")]
        public List<Server> Servers { get; private set; }

        public List<ulong> DonatorRoleList => DonatorRoles.Values.ToList();
    }

    public struct DatabaseInstance
    {
        [JsonProperty("server")]
        public string Server { get; private set; }

        [JsonProperty("port")]
        public uint Port { get; private set; }

        [JsonProperty("db")]
        public string Database { get; private set; }

        [JsonProperty("user")]
        public string Username { get; private set; }

        [JsonProperty("password")]
        public string Password { get; private set; }
    }

    public struct Server
    {

        [JsonProperty("label")]
        public string Label { get; private set; }

        [JsonProperty("ip")]
        public string IP { get; private set; }

        public override string ToString() => IP;
    }
}