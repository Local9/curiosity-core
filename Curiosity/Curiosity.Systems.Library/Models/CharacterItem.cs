using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models
{
    [DataContract]
    public class CharacterItem
    {
        [DataMember(Name = "shopCategory", EmitDefaultValue = false)]
        public string ShopCategory;

        [DataMember(Name = "itemCategory", EmitDefaultValue = false)]
        public string ItemCategory;

        [DataMember(Name = "label", EmitDefaultValue = false)]
        public string Label;

        [DataMember(Name = "numberOwned", EmitDefaultValue = false)]
        public int NumberOwned;
    }
}
