using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Police.Client.Environment.Entities.Models
{
    [DataContract]
    public class PoliceConfig
    {
        [DataMember(Name = "speedLimits")]
        public List<SpeedCamera> SpeedLimits;
    }
}
