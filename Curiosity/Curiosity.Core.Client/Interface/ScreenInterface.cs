using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Interface
{
    public class ScreenInterface
    {
        public static Dictionary<VehicleClass, BlipSprite> VehicleClassBlips = new Dictionary<VehicleClass, BlipSprite>()
        {
            { VehicleClass.Boats, BlipSprite.Boat },
            { VehicleClass.Commercial, BlipSprite.PersonalVehicleCar },
            { VehicleClass.Compacts, BlipSprite.PersonalVehicleCar },
            { VehicleClass.Coupes, BlipSprite.PersonalVehicleCar },
            { VehicleClass.Cycles, BlipSprite.PersonalVehicleBike },
            { VehicleClass.Emergency, BlipSprite.PersonalVehicleCar },
            { VehicleClass.Helicopters, BlipSprite.Helicopter },
            { VehicleClass.Industrial, BlipSprite.PersonalVehicleCar },
            { VehicleClass.Military, BlipSprite.PersonalVehicleCar },
            { VehicleClass.Motorcycles, BlipSprite.PersonalVehicleBike },
            { VehicleClass.Muscle, BlipSprite.PersonalVehicleCar },
            { VehicleClass.OffRoad, BlipSprite.PersonalVehicleCar },
            { VehicleClass.Planes, BlipSprite.Plane },
            { VehicleClass.Sedans, BlipSprite.PersonalVehicleCar },
            { VehicleClass.Service, BlipSprite.PersonalVehicleCar },
            { VehicleClass.Sports, (BlipSprite)523 },
            { VehicleClass.SportsClassics, BlipSprite.PersonalVehicleCar },
            { VehicleClass.Super, BlipSprite.PersonalVehicleCar },
            { VehicleClass.SUVs, BlipSprite.PersonalVehicleCar },
            { VehicleClass.Trains, BlipSprite.PersonalVehicleCar },
            { VehicleClass.Utility, BlipSprite.PersonalVehicleCar },
            { VehicleClass.Vans, BlipSprite.PersonalVehicleCar },
            { (VehicleClass)22, (BlipSprite)531 },
        };
        
        public static Dictionary<VehicleHash, BlipSprite> VehicleBlips = new Dictionary<VehicleHash, BlipSprite>()
        {
            { VehicleHash.Rhino, BlipSprite.Tank },
            { VehicleHash.Seashark, (BlipSprite)471 },
            { VehicleHash.Seashark2, (BlipSprite)471 },
            { VehicleHash.Seashark3, (BlipSprite)471 },
        };

        public static bool TimeoutStateValue = false;
        public static void DrawText(string text, float scale, Vector2 position, Color color, bool centered = false)
        {
            API.SetTextFont(0);
            API.SetTextProportional(true);
            API.SetTextScale(scale, scale);
            API.SetTextColour(color.R, color.G, color.B, color.A);
            API.SetTextDropshadow(0, 0, 0, 0, 255);
            API.SetTextEdge(1, 0, 0, 0, 150);
            API.SetTextDropShadow();
            API.SetTextOutline();
            API.SetTextCentre(centered);
            API.BeginTextCommandDisplayText("STRING");
            API.AddTextComponentSubstringPlayerName(text);
            API.EndTextCommandDisplayText(position.X, position.Y);
        }

        public static async void Draw3DTextTimeout(float x, float y, float z, string message, int timeout = 2500, float scaleMod = 20.0f, float distanceToHide = 20f)
        {
            TimeoutState(timeout);
            while (TimeoutStateValue)
            {
                await BaseScript.Delay(0);
                Draw3DText(x, y, z, message, scaleMod, distanceToHide);
            }
        }

        public static async void TimeoutState(int timeout)
        {
            TimeoutStateValue = true;
            await BaseScript.Delay(timeout);
            TimeoutStateValue = false;
        }

        public static void Draw3DText(Vector3 pos, string message, float scaleMod = 20.0f, float distanceToHide = 20.0f, float zOffset = 1.0f)
        {
            Draw3DText(pos.X, pos.Y, pos.Z, message, scaleMod, distanceToHide, zOffset);
        }

        public static void Draw3DText(float x, float y, float z, string message, float scaleMod = 20.0f, float distanceToHide = 20.0f, float zOffset = 1.0f)
        {
            Vector3 pos = new Vector3(x, y, z);
            float distance = (float)Math.Sqrt(GameplayCamera.Position.DistanceToSquared(pos));
            float scale = ((1 / distance) * 2) * GameplayCamera.FieldOfView / scaleMod;

            if (!Game.PlayerPed.IsInRangeOf(pos, distanceToHide))
            {
                return;
            }

            API.SetTextScale(0.0f * scale, 1.1f * scale);
            API.SetTextFont(0);
            API.SetTextProportional(true);
            API.SetTextColour(255, 255, 255, 255);
            API.SetTextDropshadow(0, 0, 0, 0, 255);
            API.SetTextEdge(2, 0, 0, 0, 150);
            API.SetTextDropShadow();
            API.SetTextOutline();

            API.SetDrawOrigin(x, y, z + zOffset, 0);

            API.SetTextEntry("STRING");
            API.SetTextCentre(true);
            API.AddTextComponentString(message);

            API.EndTextCommandDisplayText(0, 0);
            API.ClearDrawOrigin();
        }

        public static void Text(string text, float scale, PointF position, Color color, CitizenFX.Core.UI.Font font = CitizenFX.Core.UI.Font.Monospace, Alignment alignment = Alignment.Center, bool shadow = false, bool outline = false)
        {

        }

        internal static async Task FadeOut(int ms = 1000)
        {
            Screen.Fading.FadeOut(ms);
            while (Screen.Fading.IsFadingOut)
            {
                await BaseScript.Delay(0);
            }
        }

        internal static async Task FadeIn(int ms = 1000)
        {
            Screen.Fading.FadeIn(ms);
            while (Screen.Fading.IsFadingIn)
            {
                await BaseScript.Delay(0);
            }
        }
    }
}