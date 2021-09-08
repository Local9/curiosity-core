using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.MissionManager.Client.Attributes;
using CitizenFX.Core.UI;

namespace Curiosity.MissionManager.Client.Managers
{
    public class TestCode : Manager<TestCode>
    {
        public override void Begin()
        {

        }

        [TickHandler]
        private async Task NodeMarkers()
        {
            try
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    Vector3 offset = new Vector3(0f, -50f, 0f);
                    Vector3 offsetInWorld = Game.PlayerPed.GetOffsetPosition(offset);
                    Vector3 vehicleNode = Vector3.Zero;
                    if (GetClosestVehicleNode(offsetInWorld.X, offsetInWorld.Y, offsetInWorld.Z, ref vehicleNode, 8, 3.0f, 0))
                    {
                        NativeUI.Notifications.ShowFloatingHelpNotification("Vehicle Node", vehicleNode);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        [TickHandler]
        private async Task Position()
        {
            try
            {
                if (Game.PlayerPed.IsAlive)
                {
                    Vector3 pos = Game.PlayerPed.Position;
                    Screen.ShowSubtitle($"{pos.X}/{pos.Y}/{pos.Z}");
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
