using Newtonsoft.Json;

namespace Curiosity.Systems.Library.Models
{
    public class CharacterStat
    {
        [JsonIgnore] public int Id;
        public string Label;
        public long Value;
    }
}
