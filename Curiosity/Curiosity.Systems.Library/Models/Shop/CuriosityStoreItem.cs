using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models.Shop
{
    [DataContract]
    public class CuriosityStoreItem
    {
        [DataMember(Name = "shopItemId")]
        public int ShopItemId;

        [DataMember(Name = "itemId")]
        public int ItemId;

        [DataMember(Name = "label")]
        public string Label;

        [DataMember(Name = "description")]
        public string Description;

        [DataMember(Name = "buyValue")]
        public int BuyValue;

        [DataMember(Name = "buyBackValue")]
        public int BuyBackValue;

        [DataMember(Name = "numberInStock")]
        public int NumberInStock;

        [DataMember(Name = "itemPurchased")]
        public bool ItemPurchased;

        [DataMember(Name = "numberOwned")]
        public int NumberOwned;

        [DataMember(Name = "isStockManaged")]
        public bool IsStockManaged;

        [DataMember(Name = "maximumAllowed")]
        public int MaximumAllowed;

        [DataMember(Name = "hasRoleRequirement")]
        public bool hasRoleRequirement;

        [DataMember(Name = "numberOfSkillRequirements")]
        public int NumberOfSkillRequirements;

        [DataMember(Name = "numberOfItemRequirements")]
        public int NumberOfItemRequirements;

        [DataMember(Name = "imageUri")]
        public string ImageUri;
    }
}
