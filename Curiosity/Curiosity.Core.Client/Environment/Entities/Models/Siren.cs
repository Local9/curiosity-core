using System.Runtime.Serialization;

namespace Curiosity.Core.Client.Environment.Entities.Models
{
    [DataContract]
    public class Siren
    {
        [DataMember(Name = "hash")]
        public string Hash;

        [DataMember(Name = "sirens")]
        public List<string> Sirens = new List<string>();

        [DataMember(Name = "warning")]
        public string WarningHorn;
    }
}
