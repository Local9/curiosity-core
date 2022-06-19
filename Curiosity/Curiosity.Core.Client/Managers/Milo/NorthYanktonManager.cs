using Curiosity.Systems.Library.Models;
using System.Drawing;

namespace Curiosity.Core.Client.Managers.Milo
{
    public class NorthYanktonManager : Manager<NorthYanktonManager>
    {
        Position posLosSantos1 = new Position();

        Position posNorthYankton = new Position();

        Color markerColor = Color.FromArgb(255, 135, 206, 235);
        Vector3 markerScale = new Vector3(1.5f, 1.5f, .5f);
        Vector3 markerScaleVehicle = new Vector3(10f, 10f, 1f);

        NUIMarker markerLs1;
        NUIMarker markerNy1;

        public override void Begin()
        {

        }
    }
}
