using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Threading.Tasks;

namespace Curiosity.Client.Net
{
    public class CuriosityPlayer : BaseScript
    {
        long userId = 0;

        public CuriosityPlayer()
        {
            EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
            EventHandlers["curiosity:Client:Player:Setup"] += new Action<long>(OnPlayerSetup);

            Tick += OnGetUserId;
        }

        private void OnResourceStart(string resourceName)
        {
            Screen.ShowNotification("~b~Info:~w~ Curiosity Server Started");

            if (API.GetCurrentResourceName() != resourceName) return;

            Debug.WriteLine("curiosity-server -> Started");

            Screen.ShowNotification("~b~Info:~w~ Curiosity Server Started");
        }

        async Task OnGetUserId()
        {
            while (userId == 0)
            {
                await Delay(1000);
                TriggerServerEvent("curiosity:Server:Player:Setup");
            }
            await Delay(1000);
        }

        async void OnPlayerSetup(long userId)
        {
            this.userId = userId;

            float left = (Screen.Width / 2) / 3.2f;

            Text text = new Text($"NAME: {Game.Player.Name}\nPLAYERID: {userId}", new System.Drawing.PointF { X = left, Y = Screen.Height - 37 }, 0.3f, System.Drawing.Color.FromArgb(50, 255, 255, 255), Font.ChaletComprimeCologne, Alignment.Left, false, true);
            text.WrapWidth = 300;
            await Delay(0);

            while (true)
            {
                text.Draw();
                await Delay(0);
            }

        }
    }
}
