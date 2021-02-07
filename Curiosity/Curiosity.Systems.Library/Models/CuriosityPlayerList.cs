using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models
{
    [DataContract]
    public class CuriosityPlayerList
    {
        [DataMember(Name = "id", Order = 0)]
        public long UserId;

        [DataMember(Name = "handle", Order = 1)]
        public int ServerHandle;

        [DataMember(Name = "name", Order = 2)]
        public string Name;

        [DataMember(Name = "ping", Order = 3)]
        public int Ping;

        [DataMember(Name = "job", Order = 3)]
        public string Job;
    }
}
