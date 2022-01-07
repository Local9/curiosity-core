using CitizenFX.Core;
using Curiosity.Systems.Library.Models;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Curiosity.Core.Client.Environment.Entities.Models.Config
{
    [DataContract]
    public class PoliceConfigFile
    {
        [DataMember(Name = "speedCameraDistance")]
        public float SpeedCameraDistance;

        [DataMember(Name = "speedLimits")]
        public Dictionary<string, int> SpeedLimits;

        [DataMember(Name = "ignoredVehicles")]
        public List<string> IgnoredVehicles;

        [DataMember(Name = "speedCameras")]
        public List<PoliceCamera> SpeedCameras;
    }

    [DataContract]
    public class PoliceCamera : SpeedCamera
    {
        public bool Saved;
        public Vector3 Position => new Vector3(X, Y, Z);
        public bool Active { get; internal set; }
    }
}
