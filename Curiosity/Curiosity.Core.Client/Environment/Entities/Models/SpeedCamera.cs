using Curiosity.Core.Client.Utils;
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

        [DataMember(Name = "width", EmitDefaultValue = false)]
        public float? Width;

        [DataMember(Name = "start", EmitDefaultValue = false)]
        public Vector Start = new Vector();

        [DataMember(Name = "end", EmitDefaultValue = false)]
        public Vector End = new Vector();

        public Vector3 Center => (Start.Vector3 + End.Vector3) / 2;
        public string Direction => Common.GetCompassHeading(Start.Vector3, End.Vector3);

        public void AddStart(Vector3 v)
        {
            Vector vector = new Vector();
            vector.X = v.X;
            vector.Y = v.Y;
            vector.Z = v.Z;
            Start = vector;
        }

        public void AddEnd(Vector3 v)
        {
            Vector vector = new Vector();
            vector.X = v.X;
            vector.Y = v.Y;
            vector.Z = v.Z;
            End = vector;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    [DataContract]
    public class Vector
    {
        [DataMember(Name = "X")]
        public float X = 0;

        [DataMember(Name = "Y")]
        public float Y = 0;

        [DataMember(Name = "Z")]
        public float Z = 0;

        public Vector3 Vector3 => new Vector3(X, Y, Z);
    }
}
