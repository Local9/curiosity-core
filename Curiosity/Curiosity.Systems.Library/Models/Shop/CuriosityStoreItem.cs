using System.Collections.Generic;
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
        public bool HasRoleRequirement;

        [DataMember(Name = "numberOfSkillRequirements")]
        public int NumberOfSkillRequirements;

        [DataMember(Name = "numberOfItemRequirements")]
        public int NumberOfItemRequirements;

        [DataMember(Name = "imageUri")]
        public string ImageUri;

        [DataMember(Name = "skillRequirements")]
        public List<SkillRequirement> SkillRequirements = new List<SkillRequirement>();

        [DataMember(Name = "itemRequirements")]
        public List<ItemRequirement> ItemRequirements = new List<ItemRequirement>();

        [DataMember(Name = "roleRequirements")]
        public List<RoleRequirement> RoleRequirements = new List<RoleRequirement>();
    }

    [DataContract]
    public class SkillRequirement
    {
        public string SkillLabel;
        public int ExperienceRequired;
        public int ExperienceCurrent;
        public bool RequirementMet;
    }

    [DataContract]
    public class ItemRequirement
    {
        public string ItemLabel;
        public int AmountRequired;
        public int AmountCurrent;
        public bool RequirementMet;
    }

    [DataContract]
    public class RoleRequirement
    {
        public int RoleId;
        public string Description;
        public bool HasRole;
    }
}
