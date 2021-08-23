using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models
{
    [DataContract]
    public class ClientConfig
    {
        [DataMember(Name = "suppressedVehicles")]
        public List<string> VehiclesToSuppress;

        [DataMember(Name = "supporter")]
        public Supporter Supporter;
    }

    [DataContract]
    public class Supporter
    {
        [DataMember(Name = "companions")]
        public List<Companion> Companions;

        [DataMember(Name = "models")]
        public List<SupporterModel> SupporterModels;
    }


    [DataContract]
    public class Companion
    {
        [DataMember(Name = "label")]
        public string Label;

        [DataMember(Name = "hash")]
        public string Hash;
    }


    [DataContract]
    public class SupporterModel
    {
        [DataMember(Name = "label")]
        public string Label;

        [DataMember(Name = "hash")]
        public string Hash;
    }
}
