using Curiosity.Systems.Library.Enums;
using Newtonsoft.Json;
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
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    [DataContract]
    public class CuriosityShopItem : CuriosityItem
    {
        [DataMember(Name = "shopItemId")]
        public int? ShopItemId;

        [DataMember(Name = "categoryId")]
        public int? CategoryId;

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

        [DataMember(Name = "spawnTypeId")]
        public SpawnType SpawnTypeId;

        public bool IsVehicle => SpawnTypeId == SpawnType.Helicopter
            || SpawnTypeId == SpawnType.Plane
            || SpawnTypeId == SpawnType.Boat
            || SpawnTypeId == SpawnType.Vehicle;

        [DataMember(Name = "isHealingItem")]
        public bool IsHealingItem;

        [DataMember(Name = "healingAmount")]
        public int HealingAmount;

        [DataMember(Name = "originalValue")]
        public int OriginalValue;

        [DataMember(Name = "canCarry")]
        public bool CanCarry = false;

        [DataMember(Name = "carryingMaxed")]
        public bool CarringMaxed = false;

        [DataMember(Name = "skillRequirements")]
        public List<SkillRequirement> SkillRequirements = new List<SkillRequirement>();

        [DataMember(Name = "itemRequirements")]
        public List<ItemRequirement> ItemRequirements = new List<ItemRequirement>();

        [DataMember(Name = "roleRequirements")]
        public List<RoleRequirement> RoleRequirements = new List<RoleRequirement>();

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
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
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
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
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
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
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    [DataContract]
    public class CharacterKit
    {
        [DataMember(Name = "itemId")]
        public int ItemId;

        [DataMember(Name = "itemCategoryId")]
        public int ItemCategoryId;

        [DataMember(Name = "numberOwned")]
        public int NumberOwned;

        [DataMember(Name = "amount")]
        public int Amount;
    }
}
