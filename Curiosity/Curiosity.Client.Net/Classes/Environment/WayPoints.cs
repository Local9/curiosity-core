using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using CitizenFX.Core.Native;

using Curiosity.Shared.Client.net.Helper;

namespace Curiosity.Client.net.Classes.Environment
{
    class WayPoints
    {
        static Client client = Client.GetInstance();

        static Dictionary<int, Marker> waypoints = new Dictionary<int, Marker>();
        static Dictionary<int, Ped> racers = new Dictionary<int, Ped>();

        public static void Init()
        {
            client.RegisterTickHandler(ControlHandler);
            client.RegisterTickHandler(ShowCheckpoints);
            client.RegisterTickHandler(RacePed);
        }

        static async Task ControlHandler()
        {
            if (ControlHelper.IsControlJustPressed(Control.Context, true, Shared.Client.net.Enums.ControlModifier.Shift))
            {
                Marker marker = new Marker(Game.PlayerPed.Position);
                if (waypoints.Count > 50)
                {
                    Screen.ShowNotification($"~r~Position ignored, max way points.");
                }
                else
                {
                    waypoints.Add(waypoints.Count + 1, marker);
                    Screen.ShowNotification($"~g~Position added.");
                }
            }
            if (ControlHelper.IsControlJustPressed(Control.Context, true, Shared.Client.net.Enums.ControlModifier.Alt))
            {
                Model model = new Model(PedHash.Abigail);

                await model.Request(50);

                while (!model.IsLoaded)
                    await model.Request(50);

                Ped ped = await World.CreatePed(model, Game.PlayerPed.Position);
                racers.Add(ped.Handle, ped);
                
                Screen.ShowNotification($"~g~Ped added.");
            }
            await Task.FromResult(0);
        }

        static async Task ShowCheckpoints()
        {
            foreach (KeyValuePair<int, Marker> checkpoint in waypoints)
            {
                Marker marker = checkpoint.Value;
                World.DrawMarker(marker.Type, marker.Position, marker.Direction, marker.Rotation, marker.Scale, marker.Color);
            }
            await Task.FromResult(0);
        }

        static async Task RacePed()
        {
            foreach(KeyValuePair<int, Ped> item in racers)
            {
                Ped ped = item.Value;
                GotoWaypoints(ped);
                await Client.Delay(0);
            }
        }

        static async void GotoWaypoints(Ped ped)
        {
            int currentIndex = 1;
            Vector3 currentWaypoint = waypoints[currentIndex].Position;

            while (true)
            {
                float screenX = 0.0f;
                float screenY = 0.0f;
                API.World3dToScreen2d(ped.Position.X, ped.Position.Y, ped.Position.Z, ref screenX, ref screenY);

                UI.UI.DrawText($"IDX: {currentIndex}\nWP: {currentWaypoint}", new Vector2 { X = screenX, Y = screenY }, System.Drawing.Color.FromArgb(255, 255, 255, 255), 0.3f, Font.ChaletComprimeCologne, Alignment.Center);

                if (NativeWrappers.GetDistanceBetween(ped.Position, currentWaypoint, true) < 3.0f)
                {
                    ped.Task.RunTo(currentWaypoint);
                }
                else
                {
                    currentIndex++;

                    if (currentIndex > waypoints.Count)
                        currentIndex = 1;

                    currentWaypoint = waypoints[currentIndex].Position;
                }
                await Client.Delay(10);
            }
        }
    }
}
