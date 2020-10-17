using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Missions.Client.Classes.PlayerClient;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using static CitizenFX.Core.Native.API;
using Font = CitizenFX.Core.UI.Font;

namespace Curiosity.Missions.Client.Wrappers
{
    internal class Helpers
    {
        private static readonly Vector2 DefaultPos = new Vector2(0.6f, 0.5f);
        public static readonly Color DefaultColor = Color.FromArgb(255, 255, 255);

        static public void ShowSimpleNotification(string message)
        {
            Screen.ShowNotification(message);
        }

        static public void ShowNotification(string title, string subtitle, string message, NotificationCharacter notificationCharacter = NotificationCharacter.CHAR_CALL911)
        {
            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{notificationCharacter}", 2, title, subtitle, message, 2);
        }

        // internal methods
        static public void ShowOfficerSubtitle(string subtitle)
        {
            Screen.ShowSubtitle($"~o~Officer:~w~ {subtitle}");
        }

        static public void ShowSuspectSubtitle(string subtitle)
        {
            Screen.ShowSubtitle($"~b~Suspect:~w~ {subtitle}");
        }

        static public void DrawData(Entity entity, Dictionary<string, string> keyValuePairs)
        {
            var entityPos = entity.Position;
            var pos = WorldToScreen(entityPos);
            if (pos.X <= 0f || pos.Y <= 0f || pos.X >= 1f || pos.Y >= 1f)
            {
                pos = DefaultPos;
            }
            var dist = (float)Math.Sqrt(Game.PlayerPed.Position.DistanceToSquared(entityPos));
            var offsetX = MathUtil.Clamp((1f - dist / 100f) * 0.1f, 0f, 0.1f);
            pos.X += offsetX;

            // Draw Box
            DrawRect(pos.X + 0.12f, pos.Y, 0.24f, keyValuePairs.Count * 0.024f + 0.048f, Color.FromArgb(120, 0, 0, 0));

            var offsetY = keyValuePairs.Count * 0.012f;
            pos.Y -= offsetY;

            pos.X += 0.02f;
            // Draw data
            foreach (var entry in keyValuePairs)
            {
                if (!string.IsNullOrEmpty(entry.Value))
                    DrawText($"{entry.Key}: {entry.Value}", pos);
                pos.Y += 0.024f;
            }
        }

        public static Vector2 WorldToScreen(Vector3 position)
        {
            var screenX = new OutputArgument();
            var screenY = new OutputArgument();
            return !Function.Call<bool>(Hash._WORLD3D_TO_SCREEN2D, position.X, position.Y, position.Z, screenX, screenY) ?
                Vector2.Zero :
                new Vector2(screenX.GetResult<float>(), screenY.GetResult<float>());
        }
        public static void DrawText(string text, Vector2 pos, Color? color = null, float scale = 0.25f,
            bool shadow = false, float shadowOffset = 1f, Alignment alignment = Alignment.Left, CitizenFX.Core.UI.Font font = Font.ChaletLondon)
        {
            try
            {
                Function.Call(Hash.SET_TEXT_FONT, font);
                Function.Call(Hash.SET_TEXT_PROPORTIONAL, 0);
                Function.Call(Hash.SET_TEXT_SCALE, scale, scale);
                if (shadow)
                {
                    Function.Call(Hash.SET_TEXT_DROPSHADOW, shadowOffset, 0, 0, 0, 255);
                }
                var col = color ?? DefaultColor;
                Function.Call(Hash.SET_TEXT_COLOUR, col.R, col.G, col.B, col.A);
                Function.Call(Hash.SET_TEXT_EDGE, 1, 0, 0, 0, 255);
                Function.Call(Hash.SET_TEXT_JUSTIFICATION, alignment);
                Function.Call(Hash._SET_TEXT_ENTRY, "STRING");
                Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, text);
                Function.Call(Hash._DRAW_TEXT, pos.X, pos.Y);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public static void DrawRect(float xPos, float yPos, float xScale, float yScale, Color color)
        {
            try
            {
                Function.Call(Hash.DRAW_RECT, xPos, yPos, xScale, yScale, color.R, color.G, color.B, color.A);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public static void RequestControlOfEnt(Entity entity)
        {
            int tick = 0;
            while (!NetworkHasControlOfEntity(entity.Handle) && tick <= 25)
            {
                NetworkRequestControlOfEntity(entity.Handle);
                tick++;
            }
            if (NetworkIsSessionStarted())
            {
                int netId = entity.NetworkId;
                RequestControlOfId(netId);
                SetNetworkIdCanMigrate(netId, true);
            }
        }

        public static void RequestControlOfId(int netId)
        {
            int tick = 0;
            while (!NetworkHasControlOfEntity(netId) && tick <= 25)
            {
                if (NetworkRequestControlOfEntity(netId))
                {
                    if (ClientInformation.IsDeveloper())
                    {
                        Debug.WriteLine($"Gained Control of {netId}");
                    }
                }
                tick++;
            }
        }
    }
}
