using Curiosity.Systems.Library.Models;
using System.Drawing;

namespace Curiosity.Core.Client.Extensions.Native
{
    class NUIMarker : NativeUI.Marker
    {
        public Position TeleportPosition;
        public dynamic Data;

        public NUIMarker(MarkerType type, Vector3 position, Vector3 scale, float distance, Color color, bool bobUpDown = false, bool rotate = false, bool faceCamera = false)
            : base(type, position, scale, distance, color, bobUpDown, rotate, faceCamera)
        {

        }

        public void Add()
        {
            NativeUI.MarkersHandler.AddMarker(this);
        }

        public void Remove()
        {
            NativeUI.MarkersHandler.RemoveMarker(this);
        }
    }
}
