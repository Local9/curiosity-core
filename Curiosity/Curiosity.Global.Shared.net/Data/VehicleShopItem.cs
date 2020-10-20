using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Curiosity.Global.Shared.Data
{
    [DataContract]
    public class VehicleShopItem
    {
        [DataMember(Name = "id", IsRequired = true)]
        public int Id;

        [DataMember(Name = "label", IsRequired = true)]
        public string Label;

        [JsonIgnore]
        public string VehicleHash;

        [DataMember(Name = "cost", IsRequired = true)]
        public int Cost;

        [DataMember(Name = "numberRemaining")]
        public int? NumberRemaining;

        [DataMember(Name = "isOwned", IsRequired = true)]
        public bool IsOwned;

        [DataMember(Name = "uri")]
        public string ImageUri;
    }
}
