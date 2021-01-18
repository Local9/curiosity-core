using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models
{
    [DataContract]
    public class Marker
    {
        [DataMember(Name = "message")]
        public string Message;

        [DataMember(Name = "helpText")]
        public string HelpText;

        [DataMember(Name = "isServerEvent")]
        public bool IsServerEvent = false;

        [DataMember(Name = "isLegacyEvent")]
        public bool IsLegacyEvent = false;

        [DataMember(Name = "event")]
        public string Event;

        [DataMember(Name = "positions")]
        public List<Position> Positions;

        [DataMember(Name = "markerId")]
        public int MarkerId = 1;

        [DataMember(Name = "scale")]
        public Position Scale = new Position(1f, 1f, 1f);
        
        [DataMember(Name = "color")]
        public CuriosityColor Color;

        [DataMember(Name = "drawThreshold")]
        public float DrawThreshold = 2f;

        [DataMember(Name = "rotation")]
        public Position Rotation = new Position(0f, 0f, 0f);

        [DataMember(Name = "direction")]
        public Position Direction = new Position(0f, 0f, 0f);

        [DataMember(Name = "contextAoe")]
        public float ContextAoe = 1.5f;

        [DataMember(Name = "control")]
        public int Control = 51;
    }

    [DataContract]
    public class CuriosityColor
    {
        [DataMember(Name = "alpha")]
        public int Alpha;

        [DataMember(Name = "red")]
        public int Red;

        [DataMember(Name = "green")]
        public int Green;

        [DataMember(Name = "blue")]
        public int Blue;
    }
}
