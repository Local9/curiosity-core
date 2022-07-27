using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;

namespace Curiosity.Core.Client.Environment.Entities.Models
{
    public class SoundEmitter
    {
        [JsonProperty("Name")]
        public string Name;

        [JsonProperty("Position")]
        public Position Position;

        [JsonProperty("Flags")]
        public string Flags;

        [JsonProperty("Interior")]
        public string Interior;

        [JsonProperty("Room")]
        public string Room;

        [JsonProperty("RadioStation")]
        public string RadioStation;
    }


}
