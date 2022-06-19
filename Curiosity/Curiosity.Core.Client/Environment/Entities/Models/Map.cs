namespace Curiosity.Core.Client.Environment.Entities.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class MapObjectPosition
    {
        public float X;
        public float Y;
        public float Z;
        public Vector3 Vector3 => new(X, Y, Z);
    }

    public class Rotation
    {
        public float X;
        public float Y;
        public float Z;
        public Vector3 Vector3 => new(X, Y, Z);
    }

    public class Quaternion
    {
        public float X;
        public float Y;
        public float Z;
        public float W;
    }

    public class MapObject
    {
        public string Type;
        public MapObjectPosition Position;
        public Rotation Rotation;
        public string Hash;
        public bool Dynamic;
        public Quaternion Quaternion;
        public bool Door;
        public int PropHandle = 0;
        public int PropHash;
    }

    public class Objects
    {
        public List<MapObject> MapObject;
    }

    public class Map
    {
        public Objects Objects;
        public string RemoveFromWorld;
        public string Markers;
    }



}
