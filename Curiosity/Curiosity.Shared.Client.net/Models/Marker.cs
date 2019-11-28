using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace Curiosity.Shared.Client.net.Models
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

        public Marker(string markerMessage, Vector3 position, MarkerType type = MarkerType.VerticalCylinder, float drawThreshold = 5f)
        {
            this.Message = markerMessage;

            this.Position = position;
            // position.Z = position.Z - 0.5f;

            this.Color = System.Drawing.Color.FromArgb(255, 255, 255, 255);
            this.Type = type;
            this.Scale = 1.0f * new Vector3(1f, 1f, 1f);
            this.DrawThreshold = drawThreshold;
            this.Rotation = new Vector3(0, 0, 0);
            this.Direction = new Vector3(0, 0, 0);
        }

        public Marker(string markerMessage, Vector3 position, System.Drawing.Color color, MarkerType type = MarkerType.VerticalCylinder, float drawThreshold = 5f)
        {
            this.Message = markerMessage;

            this.Position = position;
            // position.Z = position.Z - 0.5f;
            
            this.Color = color;
            this.Type = type;
            this.Scale = 1.0f * new Vector3(1f, 1f, 1f);
            this.DrawThreshold = drawThreshold;
            this.Rotation = new Vector3(0, 0, 0);
            this.Direction = new Vector3(0, 0, 0);
        }
    }
}
