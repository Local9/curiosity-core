using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models
{
    [DataContract]
    public class GenericMessage
    {
        [DataMember(Name = "success")]
        public bool Success;

        [DataMember(Name = "message", EmitDefaultValue = false)]
        public string Message;

        [DataMember(Name = "cost", EmitDefaultValue = false)]
        public int Cost;
    }
}
