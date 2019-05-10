using CitizenFX.Core;

namespace Curiosity.Shared.Client.net.Models
{
    public class PointEvent
    {
        public float[] Position { get; set; }
        public float AoeRange { get; set; }
        public string EventName { get; set; }
        public string SerializedArguments { get; set; }
        public int SourceServerId { get; set; }
        public bool IgnoreOwnEvent { get; set; }

        public PointEvent() { }

        public PointEvent(string EventName, float[] Position, float AoeRange, string SerializedArguments, int SourceServerId, bool IgnoreOwnEvent)
        {
            this.EventName = EventName;
            this.Position = Position;
            this.AoeRange = AoeRange;
            this.SerializedArguments = SerializedArguments;
            this.SourceServerId = SourceServerId;
            this.IgnoreOwnEvent = IgnoreOwnEvent;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(this.Position[0], this.Position[1], this.Position[2]);
        }
    }
}
