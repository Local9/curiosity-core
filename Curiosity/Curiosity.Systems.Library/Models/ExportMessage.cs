using Curiosity.Systems.Library.Models.Shop;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models
{
    [DataContract]
    public class ExportMessage
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        [DataMember(Name = "success", Order = 0)]
        public bool success
        {
            get
            {
                return string.IsNullOrEmpty(error);
            }
        }

        [DataMember(Name = "error", Order = 1, EmitDefaultValue = false)]
        public string error;

        [DataMember(Name = "newNumberValue", Order = 2, EmitDefaultValue = false)]
        public long newNumberValue;

        [DataMember(Name = "skill", Order = 2, EmitDefaultValue = false)]
        public CharacterSkillExport skill;

        [DataMember(Name = "value", Order = 2, EmitDefaultValue = false)]
        public long value { get; set; }

        [DataMember(Name = "item", Order = 2, EmitDefaultValue = false)]
        public CuriosityShopItem item { get; set; }


        [DataMember(Name = "networkId", Order = 2, EmitDefaultValue = false)]
        public int networkId { get; set; }

        [DataMember(Name = "roleId", Order = 2, EmitDefaultValue = false)]
        public int roleId { get; set; }
    }
}
