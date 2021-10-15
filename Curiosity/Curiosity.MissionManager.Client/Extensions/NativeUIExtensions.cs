using CitizenFX.Core;
using Curiosity.MissionManager.Client.Environment.Entities.Models;
using System.Drawing;

namespace Curiosity.MissionManager.Client.Extensions
{
    public class NUIMarker : NativeUI.Marker
    {
        public dynamic Data;
        public Blip Blip;
        public BlipMissionInfo BlipMissionInfo;

        public NUIMarker(MarkerType type, Vector3 position, float distance, Color color, bool placeOnGround = false, bool bobUpDown = false, bool rotate = false, bool faceCamera = false) : base(type, position, distance, color, placeOnGround, bobUpDown, rotate, faceCamera)
        {

        }

        public NUIMarker(MarkerType type, Vector3 position, Vector3 scale, float distance, Color color, bool placeOnGround = false, bool bobUpDown = false, bool rotate = false, bool faceCamera = false) : base(type, position, scale, distance, color, placeOnGround, bobUpDown, rotate, faceCamera)
        {

        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
