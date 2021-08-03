using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Library.Models
{
    [DataContract]
    public class ClientConfig
    {
        [DataMember(Name = "suppressedVehicles")]
        public List<string> VehiclesToSuppress;
    }
}
