using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models
{
    [DataContract]
    public class CuriosityPlayerListItem
    {
        [DataMember(Name = "id", Order = 0)]
        public long UserId;

        [DataMember(Name = "handle", Order = 1)]
        public int ServerHandle;

        [DataMember(Name = "role", Order = 2)]
        public string Role;

        [DataMember(Name = "name", Order = 3)]
        public string Name;

        [DataMember(Name = "ping", Order = 4)]
        public int Ping;

        [DataMember(Name = "job", Order = 5)]
        public string Job;

        [DataMember(Name = "routingBucket", Order = 5)]
        public int RoutingBucket;

        [DataMember(Name = "discordId", Order = 6)]
        public ulong DiscordId;

    }
}
