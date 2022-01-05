using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models.Casino
{
    [DataContract]
    public class RouletteTable
    {
        [JsonIgnore] public int Handle;

        [DataMember(Name = "table")]
        public string Table;

        [DataMember(Name = "position")]
        public Position Position;

        [DataMember(Name = "heading")]
        public float Heading;

        [DataMember(Name = "seating")]
        public List<Position> Seating;
    }
}
