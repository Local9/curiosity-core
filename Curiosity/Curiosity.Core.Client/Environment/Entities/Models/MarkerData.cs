using CitizenFX.Core;
using Curiosity.Systems.Library.Models;

namespace Curiosity.Core.Client.Environment.Entities.Models
{
    public class MarkerData : Marker
    {
        public Vector3 Position;
        public Vector3 VScale;
        public System.Drawing.Color ColorArgb = System.Drawing.Color.FromArgb(255, 255, 255, 255);
        public Vector3 VRotation;
        public Vector3 VDirection;
    }
}
