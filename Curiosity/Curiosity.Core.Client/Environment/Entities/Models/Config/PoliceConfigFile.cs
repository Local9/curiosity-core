﻿using System.Runtime.Serialization;

namespace Curiosity.Core.Client.Environment.Entities.Models.Config
{
    [DataContract]
    public class PoliceConfigFile
    {
        [DataMember(Name = "speedCameraDistance")]
        public float SpeedCameraDistance;

        [DataMember(Name = "speedCameraWidth")]
        public float SpeedCameraWidth;

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

        public bool Active { get; internal set; }
    }
}
