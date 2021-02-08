using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models
{
    public class CharacterSkill
    {
        [JsonIgnore] public int Id;
        public string Label;
        public string Description;
        public long Value;
    }

    [DataContract]
    public class CharacterSkillExport
    {
        [DataMember(Name = "skillExperience")]
        public long SkillExperience;
        [DataMember(Name = "KnowledgeExperience")]
        public long KnowledgeExperience;
    }
}
