using CitizenFX.Core;
using Curiosity.MissionManager.Client.Environment.Enums;

namespace Curiosity.MissionManager.Client.Environment.Entities.Models
{
    public class Marker
    {
        public string Message { get; private set; }
        public Vector3 Position { get; private set; }
        public MarkerType Type { get; private set; }
        public Vector3 Scale { get; private set; }
        public System.Drawing.Color Color { get; private set; }
        public float DrawThreshold { get; private set; }

        public Vector3 Rotation { get; private set; }
        public Vector3 Direction { get; private set; }

        public MarkerFilter MarkerFilter { get; private set; }

        public Marker(string markerMessage, Vector3 position, MarkerType type = MarkerType.VerticalCylinder, float drawThreshold = 3f, MarkerFilter markerFilter = MarkerFilter.Unknown)
        {
            this.Message = markerMessage;
            this.Position = position;
            this.Color = System.Drawing.Color.FromArgb(255, 255, 255, 255);
            this.Type = type;
            this.Scale = 1.0f * new Vector3(1f, 1f, 1f);
            this.DrawThreshold = drawThreshold;
            this.Rotation = new Vector3(0, 0, 0);
            this.Direction = new Vector3(0, 0, 0);
            this.MarkerFilter = markerFilter;
        }

        public Marker(string markerMessage, Vector3 position, System.Drawing.Color color, MarkerType type = MarkerType.VerticalCylinder, float drawThreshold = 3f, MarkerFilter markerFilter = MarkerFilter.Unknown)
        {
            this.Message = markerMessage;
            this.Position = position;
            this.Color = color;
            this.Type = type;
            this.Scale = 1.0f * new Vector3(1f, 1f, 1f);
            this.DrawThreshold = drawThreshold;
            this.Rotation = new Vector3(0, 0, 0);
            this.Direction = new Vector3(0, 0, 0);
            this.MarkerFilter = markerFilter;
        }
    }
}
