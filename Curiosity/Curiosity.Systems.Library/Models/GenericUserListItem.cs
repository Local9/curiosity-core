using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models
{
    [DataContract]
    public class GenericUserListItem
    {

        [DataMember(Name = "serverId", Order = 0)]
        public int ServerId;

        [DataMember(Name = "name", Order = 1)]
        public string Name;
    }
}
