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

        [DataMember(Name = "cost", IsRequired = true)]
        public int Cost;

        [DataMember(Name = "numberRemaining", IsRequired = true)]
        public int NumberRemaining;

        [DataMember(Name = "isOwned", IsRequired = true)]
        public bool IsOwned;
    }
}
