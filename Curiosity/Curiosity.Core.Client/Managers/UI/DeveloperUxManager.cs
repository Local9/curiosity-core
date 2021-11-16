using CitizenFX.Core;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Utils;
using Curiosity.Systems.Library.Enums;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers.UI
{
    public class DeveloperUxManager : Manager<DeveloperUxManager>
    {
        public bool Enabled { get; internal set; } = false;
        public float Scale = .2f;

        public override void Begin()
        {
            
        }

        public void EnableDeveloperOverlay()
        {
            Enabled = true;
            Instance.AttachTickHandler(DeveloperOverlay);
            Instance.AttachTickHandler(DeveloperEntityOverlay);
            NotificationManager.GetModule().Success($"Enabled Developer UI");
        }

        public void DisableDeveloperOverlay()
        {
            Enabled = false;
            Instance.DetachTickHandler(DeveloperOverlay);
            Instance.DetachTickHandler(DeveloperEntityOverlay);
            NotificationManager.GetModule().Success($"Disabled Developer UI");
        }

        private async Task DeveloperOverlay()
        {
            Tuple<float, float> minimap = ScreenInterface.MinimapAnchor();
            Vector2 pos = new Vector2(minimap.Item1, minimap.Item2 + 0.185f);
            Color color = Color.FromArgb(255, 255, 255, 255);

            Vector3 playerPos = Cache.PlayerPed.Position;
            string direction = Common.GetHeadingDirection();

            ScreenInterface.DrawTextLegacy($"X: {playerPos.X}, Y: {playerPos.Y}, Z: {playerPos.Z}, H: {Cache.PlayerPed.Heading}, DIR: {direction}", Scale, pos, color);
        }

        private async Task DeveloperEntityOverlay()
        {
            Vehicle[] vehicles = World.GetAllVehicles();

            foreach(Vehicle vehicle in vehicles)
            {
                if (!vehicle.IsInRangeOf(Cache.PlayerPed.Position, 10f)) continue;

                bool spawned = vehicle.State.Get($"{StateBagKey.VEH_SPAWNED}") ?? false;
                if (spawned)
                {
                    DebugDisplay.DrawData(vehicle);
                }
            }
        }
    }
}
