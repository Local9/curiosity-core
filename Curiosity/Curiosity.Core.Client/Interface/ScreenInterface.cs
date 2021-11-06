using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using Font = CitizenFX.Core.UI.Font;

namespace Curiosity.Core.Client.Interface
{
    public class ScreenInterface
    {
        public static Dictionary<VehicleClass, int> VehicleClassBlips = new Dictionary<VehicleClass, int>()
        {
            { VehicleClass.Boats, (int)BlipSprite.Boat },
            { VehicleClass.Commercial, (int)BlipSprite.PersonalVehicleCar },
            { VehicleClass.Compacts, (int)BlipSprite.PersonalVehicleCar },
            { VehicleClass.Coupes, (int)BlipSprite.PersonalVehicleCar },
            { VehicleClass.Cycles, (int)BlipSprite.PersonalVehicleBike },
            { VehicleClass.Emergency, (int)BlipSprite.PersonalVehicleCar },
            { VehicleClass.Helicopters, (int)BlipSprite.Helicopter },
            { VehicleClass.Industrial, (int)BlipSprite.PersonalVehicleCar },
            { VehicleClass.Military, (int)BlipSprite.PersonalVehicleCar },
            { VehicleClass.Motorcycles, (int)BlipSprite.PersonalVehicleBike },
            { VehicleClass.Muscle, (int)BlipSprite.PersonalVehicleCar },
            { VehicleClass.OffRoad, (int)BlipSprite.PersonalVehicleCar },
            { VehicleClass.Planes, (int)BlipSprite.Plane },
            { VehicleClass.Sedans, (int)BlipSprite.PersonalVehicleCar },
            { VehicleClass.Service, (int)BlipSprite.PersonalVehicleCar },
            { VehicleClass.Sports, 523 },
            { VehicleClass.SportsClassics, (int)BlipSprite.PersonalVehicleCar },
            { VehicleClass.Super, (int)BlipSprite.PersonalVehicleCar },
            { VehicleClass.SUVs, (int)BlipSprite.PersonalVehicleCar },
            { VehicleClass.Trains, (int)BlipSprite.PersonalVehicleCar },
            { VehicleClass.Utility, (int)BlipSprite.PersonalVehicleCar },
            { VehicleClass.Vans, (int)BlipSprite.PersonalVehicleCar },
            { (VehicleClass)22, 531 },
        };
        
        public static Dictionary<VehicleHash, int> VehicleBlips = new Dictionary<VehicleHash, int>()
        {
            { VehicleHash.Rhino, (int)BlipSprite.Tank },
            { VehicleHash.Seashark, 471 },
            { VehicleHash.Seashark2, 471 },
            { VehicleHash.Seashark3, 471 },
            { VehicleHash.Phantom, 477 },
            { VehicleHash.Phantom2, 528 },
            { VehicleHash.Phantom3, 477 },
            { VehicleHash.Hauler, 477 },
            { VehicleHash.Hauler2, 477 },
            { VehicleHash.Bus, 513 },
            { VehicleHash.PBus, 513 },
            { VehicleHash.RentalBus, 513 },
            { VehicleHash.Dune4, 531 },
            { VehicleHash.Dune5, 531 },
        };

        public static bool TimeoutStateValue = false;

        public static void DrawText(string text, float x, float y, float scale = 0.4f, bool centered = false, int font = 4, int r  = 255, int g = 255, int b = 244, int alpha = 255)
        {
            DrawText(text, scale, new Vector2(x, y), Color.FromArgb(alpha, r, g, b), centered, (Font)font, Alignment.Right);
        }

        public static void DrawText(string text, float scale, Vector2 position, Color color, bool centered = false, Font font = Font.ChaletLondon, Alignment alignment = Alignment.Left)
        {
            SetTextFont((int)font); // 7412968334783068634L
            SetTextScale(scale, scale); // 560759698880214217L
            SetTextColour(color.R, color.G, color.B, color.A); // -4725643803099155390L
            SetTextDropshadow(10, 0, 0, 0, 192); // 2063750248883895902L
            SetTextOutline();
            SetTextCentre(centered); // -4598371208749279093L
            SetTextWrap(position.Y, position.X); // 7139434236170869360L

            SetTextJustification((int)alignment); // 5623137247512493770L

            BeginTextCommandDisplayText("STRING"); // 2736978246810207435L
            AddTextComponentSubstringPlayerName(text); // 7789129354908300458L
            EndTextCommandDisplayText(position.Y, position.X);
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

            if (!Cache.PlayerPed.IsInRangeOf(pos, distanceToHide))
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

        public static float TextWidth(string text, float size, int font = 4)
        {
            BeginTextCommandGetWidth("STRING");
            SetTextFont(font);
            SetTextScale(size, size);
            AddTextComponentSubstringPlayerName(text);
            return EndTextCommandGetWidth(true);
        }

        public static Tuple<float, float> MinimapAnchor()
        {
            SetScriptGfxAlign(76, 66); // https://runtime.fivem.net/doc/natives/?_0xB8A850F20A067EB6
            float minimapTopX = 0f;
            float minimapTopY = 0f;
            GetScriptGfxPosition(-0.0045f, 0.002f + (-0.188888f), ref minimapTopX, ref minimapTopY);
            ResetScriptGfxAlign();

            return new Tuple<float, float>(minimapTopX, minimapTopY);
        }

        public static Tuple<float, float> MinimapAnchorNui()
        {
            SetScriptGfxAlign(76, 66); // https://runtime.fivem.net/doc/natives/?_0xB8A850F20A067EB6
            float minimapTopX = 0f;
            float minimapTopY = 0f;
            GetScriptGfxPosition(-0.0045f, 0.002f + (-0.188888f), ref minimapTopX, ref minimapTopY);
            ResetScriptGfxAlign();
            int scrnW = 0;
            int scrnH = 0;
            GetActiveScreenResolution(ref scrnW, ref scrnH);
            float finX = scrnW * minimapTopX;
            float finY = scrnH * minimapTopY;

            return new Tuple<float, float>(finX, finY);
        }

        internal static async Task FadeOut(int ms = 1000)
        {
            Screen.Fading.FadeOut(ms);
            while (IsScreenFadingOut())
            {
                await BaseScript.Delay(10);
                if (Screen.Fading.IsFadedOut) break;
            }
        }

        internal static async Task FadeIn(int ms = 1000)
        {
            Screen.Fading.FadeIn(ms);
            while (IsScreenFadingIn())
            {
                await BaseScript.Delay(10);
                if (Screen.Fading.IsFadedIn) break;
            }
        }
    }
}