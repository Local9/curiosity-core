﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace Curiosity.LifeV.Bot.Entities.CitizenFX
{
    struct CitizenFxPlayers
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
    }
}
