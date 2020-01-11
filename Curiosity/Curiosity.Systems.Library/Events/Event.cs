using Newtonsoft.Json;

namespace Curiosity.Systems.Library.Events
{
    // TODO: Tokens to increase security.
    public class Event
    {
        [JsonProperty("__Seed")] public string Seed { get; set; }
        public string Target { get; set; }
        public int Sender { get; set; }
        public EventType Type { get; set; }
        public EventMetadata Metadata { get; set; }

        public Event()
        {
            Seed = Library.Seed.Generate();
            Type = EventType.Send;
            Metadata = new EventMetadata(this);
        }
    }
}