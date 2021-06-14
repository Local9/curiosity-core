using Curiosity.Systems.Library.Enums;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models.Shop
{
    [DataContract]
    public class CuriosityItem
    {
        [DataMember(Name = "itemId")]
        public int ItemId;

        [DataMember(Name = "label")]
        public string Label;

        [DataMember(Name = "description")]
        public string Description;

        [DataMember(Name = "isDroppable")]
        public bool IsDroppable;

        [DataMember(Name = "isUsable")]
        public bool IsUsable;

        [DataMember(Name = "maximumAllowed")]
        public int MaximumAllowed;

        [DataMember(Name = "hashKey")]
        public string HashKey;

        [DataMember(Name = "imageUri")]
        public string ImageUri;
    }

    [DataContract]
    public class CuriosityShopItem : CuriosityItem
    {
        [DataMember(Name = "shopItemId")]
        public int? ShopItemId;

        [DataMember(Name = "buyValue")]
        public int? BuyValue;

        [DataMember(Name = "buyBackValue")]
        public int? BuyBackValue;

        [DataMember(Name = "numberInStock")]
        public int NumberInStock;

        [DataMember(Name = "itemPurchased")]
        public bool ItemPurchased;

        [DataMember(Name = "numberOwned")]
        public int NumberOwned;

        [DataMember(Name = "isStockManaged")]
        public bool IsStockManaged;

        [DataMember(Name = "hasRoleRequirement")]
        public bool HasRoleRequirement;

        [DataMember(Name = "numberOfSkillRequirements")]
        public int NumberOfSkillRequirements;

        [DataMember(Name = "numberOfItemRequirements")]
        public int NumberOfItemRequirements;

        public SpawnType SpawnType;

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
        [DataMember(Name = "label")]
        public string SkillLabel;

        [DataMember(Name = "xpRequired")]
        public int ExperienceRequired;

        [DataMember(Name = "xpCurrent")]
        public int ExperienceCurrent;

        [DataMember(Name = "metRequirement")]
        public bool RequirementMet;
    }

    [DataContract]
    public class ItemRequirement
    {
        [DataMember(Name = "label")]
        public string ItemLabel;

        [DataMember(Name = "amtRequired")]
        public int AmountRequired;

        [DataMember(Name = "amtCurrent")]
        public int AmountCurrent;

        [DataMember(Name = "metRequirement")]
        public bool RequirementMet;
    }

    [DataContract]
    public class RoleRequirement
    {
        [DataMember(Name = "id")]
        public int RoleId;

        [DataMember(Name = "label")]
        public string Description;

        [DataMember(Name = "hasRole")]
        public bool HasRole;
    }
}
