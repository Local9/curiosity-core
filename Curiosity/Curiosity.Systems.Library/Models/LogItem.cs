using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models
{
    [DataContract]
    public class LogItem
    {
        [DataMember(Name = "logTypeId")]
        public int LogTypeId;

        [DataMember(Name = "group")]
        public string Group;

        [DataMember(Name = "description")]
        public string Description;

        [DataMember(Name = "playerHandle", EmitDefaultValue = true)]
        public int PlayerHandle;
    }
}
