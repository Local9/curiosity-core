using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models
{
    [DataContract]
    public class ScenarioItem
    {
        [DataMember(Name = "scenario")]
        public string Scenario;

        [DataMember(Name = "enabled")]
        public bool Enabled;
    }
}
