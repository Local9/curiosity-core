using CitizenFX.Core;
using Curiosity.Core.Client.Interface;
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
            NotificationManger.GetModule().Success($"Enabled Developer UI");
        }

        public void DisableDeveloperOverlay()
        {
            Enabled = false;
            Instance.DetachTickHandler(DeveloperOverlay);
            NotificationManger.GetModule().Success($"Disabled Developer UI");
        }

        private async Task DeveloperOverlay()
        {
            Tuple<float, float> minimap = ScreenInterface.MinimapAnchor();
            Vector2 pos = new Vector2(minimap.Item1, minimap.Item2 + 0.185f);
            Color color = Color.FromArgb(255, 255, 255, 255);

            Vector3 playerPos = Cache.PlayerPed.Position;

            ScreenInterface.DrawText($"X: {playerPos.X}, Y: {playerPos.Y}, Z: {playerPos.Z}, H: {Cache.PlayerPed.Heading}", Scale, pos, color);
        }
    }
}
