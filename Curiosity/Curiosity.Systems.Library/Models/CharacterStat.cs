using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models
{
    [DataContract]
    public class CharacterStat
    {
        [DataMember(Name = "id")]
        public int Id;

        [DataMember(Name = "label")]
        public string Label;

        [DataMember(Name = "value")]
        public long Value;
    }
}
