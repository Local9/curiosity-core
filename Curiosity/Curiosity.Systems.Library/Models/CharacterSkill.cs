using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models
{
    [DataContract]
    public class CharacterSkill
    {
        [JsonIgnore] public int Id;

        [DataMember(Name = "label")]
        public string Label;

        [DataMember(Name = "description")]
        public string Description;

        [DataMember(Name = "value")]
        public long Value;
    }

    [DataContract]
    public class CharacterSkillExport
    {
        [DataMember(Name = "skillExperience")]
        public long SkillExperience = 0;

        [DataMember(Name = "knowledgeExperience")]
        public long KnowledgeExperience = 0;
    }
}
