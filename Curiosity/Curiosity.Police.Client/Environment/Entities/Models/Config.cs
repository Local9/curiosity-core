using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Curiosity.Police.Client.Environment.Entities.Models
{
    [DataContract]
    public class PoliceConfig
    {
        [DataMember(Name = "speedCameraDistance")]
        public float SpeedCameraDistance;

        [DataMember(Name = "speedLimits")]
        public Dictionary<string, int> SpeedLimits;

        [DataMember(Name = "ignoredVehicles")]
        public List<string> IgnoredVehicles;
    }

    [DataContract]
    public class SpeedCamera
    {
        [DataMember(Name = "X")]
        public float X;

        [DataMember(Name = "Y")]
        public float Y;

        [DataMember(Name = "Z")]
        public float Z;

        [DataMember(Name = "direction")]
        public string Direction;
    }
}
