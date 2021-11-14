using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Curiosity.Police.Client.Environment.Entities.Models
{
    [DataContract]
    public class PoliceConfig
    {
        [DataMember(Name = "speedLimits")]
        public Dictionary<string, int> SpeedLimits;
    }
}
