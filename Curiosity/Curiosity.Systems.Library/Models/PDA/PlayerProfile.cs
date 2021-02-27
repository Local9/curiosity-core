using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models.PDA
{
    [DataContract]
    public class PlayerProfile
    {
        [DataMember(Name = "name")]
        public string Name;

        [DataMember(Name = "role")]
        public string Role;

        [DataMember(Name = "userId")]
        public long? UserID;

        [DataMember(Name = "wallet")]
        public long? Wallet;

        [DataMember(Name = "isAdmin")]
        public bool IsAdmin;

        [DataMember(Name = "isStaff")]
        public bool IsStaff;

        [DataMember(Name = "headshot")]
        public string Headshot;

        [DataMember(Name = "worldName")]
        public string WorldName;
    }
}
