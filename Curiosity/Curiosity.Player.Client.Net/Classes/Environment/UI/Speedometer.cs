using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Client.net.Classes.Vehicle;
using System;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment.UI
{
    static class Speedometer
    {
        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
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



                    Vector2 position = new Vector2(0.167f, 0.915f);

                    string speedometerText = $"SPEED: {2.24 * Game.PlayerPed.CurrentVehicle.Speed:0} mph / {3.60 * Game.PlayerPed.CurrentVehicle.Speed:0} kph";
                    UI.DrawText(speedometerText, position, color, 0.3f, Font.ChaletComprimeCologne);
                    //if (CruiseControl.CruiseControlActive && !CruiseControl.CruiseControlDisabled)
                    //{
                    //    float speedometerWidth = Location.GetTextWidth(speedometerText, CitizenFX.Core.UI.Font.ChaletLondon, 0.25f);
                    //    FamilyRP.Roleplay.Client.UI.DrawText($"(CRUISE CONTROL)", new Vector2(0.18f + speedometerWidth, 0.93f), Color.FromArgb(160, 200, 200, 0), 0.25f);
                    //}
                    // Exclude bikes & trains from fuel -- TODO: exclude electric cars?
                    if (FuelManager.vehicleFuel >= 0f && !Game.PlayerPed.CurrentVehicle.Model.IsBicycle && !Game.PlayerPed.CurrentVehicle.Model.IsTrain && !Game.PlayerPed.CurrentVehicle.Model.IsBoat)
                    {
                        position = new Vector2(0.167f, 0.899f);
                        UI.DrawText($"FUEL: {FuelManager.vehicleFuel:0.0}%", position, color, 0.3f, Font.ChaletComprimeCologne);
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
