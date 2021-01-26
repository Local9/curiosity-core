using Newtonsoft.Json;

namespace Curiosity.Systems.Library.Models
{
    public class CharacterSkill
    {
        [JsonIgnore] public int Id;
        public string Label;
        public string Description;
        public long Value;
    }
}
