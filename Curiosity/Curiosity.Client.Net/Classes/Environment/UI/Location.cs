using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment.UI
{
    public static class Location
    {
        public static string CompactLocation { get; set; }
        public static bool HideLocation = false;
        static Client client = Client.GetInstance();

        // version can be 1 or 2
        // (This is temporary for testing variations only)
        static private int version = 2;

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:UI:LocationHide", new Action<bool>(LocationDisplayState));
        }

        static void LocationDisplayState(bool state)
        {
            HideLocation = state;
        }

        //[SecurityCritical]
        static public string GetCrossingName(Vector3 position)
        {
            OutputArgument crossingHash = new OutputArgument();
            Function.Call(Hash.GET_STREET_NAME_AT_COORD, position.X, position.Y, position.Z, new OutputArgument(), crossingHash);

            return Function.Call<string>(Hash.GET_STREET_NAME_FROM_HASH_KEY, crossingHash.GetResult<int>());
        }

        static public async Task OnShowStreetNameTick()
        {
            if (CinematicMode.DoHideHud)
                return;

            if (HideLocation)
                return;

            string currentStreetName = World.GetStreetName(Game.PlayerPed.Position);
            string currentCrossingName = GetCrossingName(Game.PlayerPed.Position);
            string localizedZone = World.GetZoneLocalizedName(Game.PlayerPed.Position);

            // Used for AI alerting
            CompactLocation = $"{currentStreetName} | {currentCrossingName} | {localizedZone}";

            if (version == 1)
            {
                CitizenFX.Core.UI.Font font = CitizenFX.Core.UI.Font.ChaletLondon;
                float scale = 0.25f;
                float x = 0.18f;
                float y = 0.95f;
                System.Drawing.Color colorLowImportance = System.Drawing.Color.FromArgb(255, 200, 200, 200);
                System.Drawing.Color colorFocus = System.Drawing.Color.FromArgb(255, 255, 255, 255);

                string textAddition;

                textAddition = "In";
                UI.DrawText(textAddition, new Vector2(x, y), colorLowImportance, 0.25f, 0, false);
                x += GetTextWidth(textAddition, font, scale);

                textAddition = $"{localizedZone}";
                UI.DrawText(textAddition, new Vector2(x, y), colorFocus, 0.25f, 0, false);
                x += GetTextWidth(textAddition, font, scale);

                if (String.IsNullOrWhiteSpace(currentCrossingName))
                {
                    textAddition = "on";
                    UI.DrawText(textAddition, new Vector2(x, y), colorLowImportance, 0.25f, 0, false);
                    x += GetTextWidth(textAddition, font, scale);

                    textAddition = $"{currentStreetName}";
                    UI.DrawText(textAddition, new Vector2(x, y), colorFocus, 0.25f, 0, false);
                    x += GetTextWidth(textAddition, font, scale);
                }
                else
                {
                    textAddition = "at crossing of";
                    UI.DrawText(textAddition, new Vector2(x, y), colorLowImportance, 0.25f, 0, false);
                    x += GetTextWidth(textAddition, font, scale);

                    textAddition = $"{currentStreetName}";
                    UI.DrawText(textAddition, new Vector2(x, y), colorFocus, 0.25f, 0, false);
                    x += GetTextWidth(textAddition, font, scale);

                    textAddition = "and";
                    UI.DrawText(textAddition, new Vector2(x, y), colorLowImportance, 0.25f, 0, false);
                    x += GetTextWidth(textAddition, font, scale);

                    textAddition = $"{currentCrossingName}";
                    UI.DrawText(textAddition, new Vector2(x, y), colorFocus, 0.25f, 0, false);
                    x += GetTextWidth(textAddition, font, scale);
                }

                textAddition = "looking";
                UI.DrawText(textAddition, new Vector2(x, y), colorLowImportance, 0.25f, 0, false);
                x += GetTextWidth(textAddition, font, scale);

                textAddition = $"{Compass.GetCardinalDirection()}";
                UI.DrawText(textAddition, new Vector2(x, y), colorFocus, 0.25f, 0, false);
                x += GetTextWidth(textAddition, font, scale);
            }
            else if (version == 2)
            {
                CitizenFX.Core.UI.Font font = CitizenFX.Core.UI.Font.ChaletLondon;
                float scale = 0.25f;
                System.Drawing.Color colorLowImportance = System.Drawing.Color.FromArgb(255, 200, 200, 200);
                System.Drawing.Color colorFocus = System.Drawing.Color.FromArgb(255, 255, 255, 255);

                string textAddition;

                Vector2 topRightCardinal = new Vector2(1.01f, 0.94f);
                Vector2 topLeftCardinal = new Vector2(topRightCardinal.X - 0.04f, topRightCardinal.Y);
                UI.DrawText($"{Compass.GetCardinalDirection()}", topLeftCardinal, colorFocus, 1f, 0, Alignment.Left);
                UI.DrawText($"{localizedZone}", new Vector2(topLeftCardinal.X - GetTextWidth($"{localizedZone}", 0, 0.35f), topLeftCardinal.Y + 0.012f), colorFocus, 0.35f, 0, Alignment.Left);

                float StreetNameWidth = GetTextWidth($"{currentStreetName}", 0, 0.35f);

                if (String.IsNullOrWhiteSpace(currentCrossingName))
                {
                    UI.DrawText($"{currentStreetName}", new Vector2(topLeftCardinal.X - StreetNameWidth, topLeftCardinal.Y + 0.032f), colorFocus, 0.35f, 0, Alignment.Left);
                }
                else
                {
                    float AmpersandWidth = GetTextWidth($"&", 0, 0.3f);
                    float CrossingNameWidth = GetTextWidth($"{currentCrossingName}", 0, 0.35f);

                    UI.DrawText($"{currentStreetName}", new Vector2(topLeftCardinal.X - StreetNameWidth, topLeftCardinal.Y + 0.032f), colorFocus, 0.35f, 0, Alignment.Left);
                    UI.DrawText($"&", new Vector2(topLeftCardinal.X - StreetNameWidth - AmpersandWidth, topLeftCardinal.Y + 0.034f), colorLowImportance, 0.3f, 0, Alignment.Left);
                    UI.DrawText($"{currentCrossingName}", new Vector2(topLeftCardinal.X - StreetNameWidth - AmpersandWidth - CrossingNameWidth, topLeftCardinal.Y + 0.032f), colorFocus, 0.35f, 0, Alignment.Left);
                }
            }

            await Task.FromResult(0);
        }

        public static float GetTextWidth(string Text, CitizenFX.Core.UI.Font Font, float Scale)
        {
            Function.Call(Hash._SET_TEXT_ENTRY_FOR_WIDTH, "jamyfafi");
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, Text);
            Function.Call(Hash.SET_TEXT_FONT, Font);
            Function.Call(Hash.SET_TEXT_SCALE, Scale, Scale);
            float result = Function.Call<float>(Hash._GET_TEXT_SCREEN_WIDTH, 1);
            return result;
        }
    }
}
