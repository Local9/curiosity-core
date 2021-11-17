using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models
{
    [DataContract]
    public class SpeedCameraMetadata
    {
        [DataMember(Name = "speedCameras")]
        public List<SpeedCamera> cameras = new List<SpeedCamera>();
    }

    [DataContract]
    public class SpeedCamera
    {
        [JsonIgnore] public string Street;

        [DataMember(Name = "limit", EmitDefaultValue = false)]
        public float? Limit;

        [DataMember(Name = "X")]
        public float X;

        [DataMember(Name = "Y")]
        public float Y;

        [DataMember(Name = "Z")]
        public float Z;

        [DataMember(Name = "direction")]
        public string Direction;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
