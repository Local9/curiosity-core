using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models.Casino
{
    [DataContract]
    public class CasinoData
    {
        [DataMember(Name = "rouletteTables")]
        public List<RouletteTable> RouletteTables;
    }
}
