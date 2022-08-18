namespace Perseverance.Discord.Bot.Entities
{
    struct CitizenFxPlayer
    {
        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("identifiers")]
        public List<string> Identifiers { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("ping")]
        public int Ping { get; set; }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
