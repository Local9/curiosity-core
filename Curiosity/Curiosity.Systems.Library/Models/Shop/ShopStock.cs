using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models.Shop
{
    [DataContract]
    public class ShopStock
    {
        [DataMember(Name = "shopCategory", EmitDefaultValue = false)]
        public string ShopCategory;

        [DataMember(Name = "itemCategory", EmitDefaultValue = false)]
        public string ItemCategory;

        [DataMember(Name = "label", EmitDefaultValue = false)]
        public string Label;

        [DataMember(Name = "numberInStock", EmitDefaultValue = false)]
        public int NumberInStock;

        [DataMember(Name = "isLow", EmitDefaultValue = false)]
        public bool IsLow;

        [DataMember(Name = "isOverStocked", EmitDefaultValue = false)]
        public bool IsOverStocked;

    }
}
