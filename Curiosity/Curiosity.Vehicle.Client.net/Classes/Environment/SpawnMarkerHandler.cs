using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Vehicle.Client.net.Classes.Environment
{
    class SpawnMarkerHandler
    {
        public static void Init()
        {
            MarkerHandler.All.Add(1, new Marker(new CitizenFX.Core.Vector3(-1108.226f, -847.1646f, 19.31689f), CitizenFX.Core.MarkerType.CarSymbol, System.Drawing.Color.FromArgb(255, 0, 0, 255), 1.0f));
        }
    }
}
