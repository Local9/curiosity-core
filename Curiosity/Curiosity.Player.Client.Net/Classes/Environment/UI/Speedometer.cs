using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using Curiosity.Client.net.Classes.Vehicle;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Curiosity.Client.net.Classes.Environment.UI
{
    class SpeedoData
    {
        public bool showhud;
        public double speed;
    }

    static class Speedometer
    {
        static Client client = Client.GetInstance();
        static SpeedoData speedoData = new SpeedoData();

        static public void Init()
        {
            client.RegisterTickHandler(OnTick);
        }

        static public async Task OnTick()
        {
            if (CinematicMode.DoHideHud) return;
            try
            {
                if (Game.PlayerPed.IsInVehicle())
                {

                    var color = System.Drawing.Color.FromArgb(255, 255, 255, 255);
                    if (Game.PlayerPed.CurrentVehicle.Speed < 0.2) // Basically stationary; it is sort of glitching when still
                    {
                        color = System.Drawing.Color.FromArgb(160, 255, 255, 255);
                    }

                    Vector2 position = new Vector2(0.167f, 0.867f);

                    if (!Game.PlayerPed.CurrentVehicle.Model.IsBicycle && !Game.PlayerPed.CurrentVehicle.Model.IsTrain && !Game.PlayerPed.CurrentVehicle.Model.IsBoat)
                    {
                        position = new Vector2(0.167f, 0.899f);

                        if (Screen.Resolution.Width > 1980)
                        {
                            position = new Vector2(0.001f, 0.899f);
                        }

                        UI.DrawText($"BODY: {Game.PlayerPed.CurrentVehicle.BodyHealth:0.0}%", position, color, 0.3f, Font.ChaletComprimeCologne);

                        position = new Vector2(0.167f, 0.915f);

                        if (Screen.Resolution.Width > 1980)
                        {
                            position = new Vector2(0.001f, 0.915f);
                        }

                        UI.DrawText($"ENGINE: {Game.PlayerPed.CurrentVehicle.EngineHealth:0.0}%", position, color, 0.3f, Font.ChaletComprimeCologne);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log.Info($"[SPEEDOMETER] {ex.GetType().ToString()} thrown");
            }
            await Task.FromResult(0);
        }
    }
}
