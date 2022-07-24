using Newtonsoft.Json;

namespace Curiosity.Core.Server.Environment
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class EventAward
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class RandomEvent
    {
        private int _cooldown = 0;

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cooldown")]
        public int Cooldown {
            get
            {
                return _cooldown;
            }
            set
            {
                _cooldown = (1000 * 60) * value;
            }
        }

        [JsonProperty("position")]
        public EventPosition Position { get; set; }

        [JsonProperty("peds")]
        public List<EventPed> Peds { get; set; }

        [JsonProperty("requirements")]
        public EventRequirement Requirements { get; set; }

        [JsonProperty("awards")]
        public List<EventAward> Awards { get; set; }

        public long GameTimeActivated;
    }

    public class EventPed
    {
        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("position")]
        public EventPosition Position { get; set; }
    }

    public class EventPosition
    {
        [JsonProperty("X")]
        public double X { get; set; }

        [JsonProperty("Y")]
        public double Y { get; set; }

        [JsonProperty("Z")]
        public double Z { get; set; }

        [JsonProperty("H")]
        public double H { get; set; }
    }

    public class EventRequirement
    {
        [JsonProperty("requirement")]
        public string Requirement { get; set; }
    }

    public class RandomEventConfig
    {
        [JsonProperty("events")]
        public List<RandomEvent> Events { get; set; }
    }


}
