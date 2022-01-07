using CitizenFX.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Curiosity.Core.Client.Environment.Entities.Models
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

        [DataMember(Name = "start", EmitDefaultValue = false)]
        public Vector Start;

        [DataMember(Name = "end", EmitDefaultValue = false)]
        public Vector End;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    [DataContract]
    public class Vector
    {
        [DataMember(Name = "X")]
        public float X;

        [DataMember(Name = "Y")]
        public float Y;

        [DataMember(Name = "Z")]
        public float Z;

        public Vector3 Vector3 => new Vector3(X, Y, Z);
    }
}
