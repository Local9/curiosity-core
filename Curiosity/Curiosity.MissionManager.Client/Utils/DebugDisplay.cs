using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.MissionManager.Client.Utils
{
    static class DebugDisplay
    {
        private static readonly Vector2 DefaultPos = new Vector2(0.6f, 0.5f);
        public static readonly Color DefaultColor = Color.FromArgb(255, 255, 255);

        public static void DrawData(this Entity entity)
        {
            var entityPos = entity.Position;

            if (entityPos.Distance(Game.PlayerPed.Position) > 20f) return;

            var pos = WorldToScreen(entityPos);
            if (pos.X <= 0f || pos.Y <= 0f || pos.X >= 1f || pos.Y >= 1f)
            {
                pos = DefaultPos;
            }
            var dist = (float)Math.Sqrt(Game.PlayerPed.Position.DistanceToSquared(entityPos));
            var offsetX = MathUtil.Clamp((1f - dist / 100f) * 0.1f, 0f, 0.1f);
            pos.X += offsetX;

            var data = GetDataFor(entity);

            // Draw Box
            DrawRect(pos.X + 0.12f, pos.Y, 0.24f, data.Count * 0.024f + 0.048f, Color.FromArgb(120, 0, 0, 0));

            var offsetY = data.Count * 0.012f;
            pos.Y -= offsetY;

            pos.X += 0.02f;
            // Draw data
            foreach (var entry in data)
            {
                if (!string.IsNullOrEmpty(entry.Value))
                    DrawText($"{entry.Key}: {entry.Value}", pos);
                pos.Y += 0.024f;
            }
        }

        private static Dictionary<string, string> GetDataFor(this Entity entity)
        {
            var list = new Dictionary<string, string>();
            try
            {
                var pos = entity.Position;
                var rot = entity.Rotation;
                var vel = entity.Velocity;
                list["Model Name"] = GetModelName(entity.Model);
                list["Model Hash"] = $"{entity.Model.Hash}";
                list["Model Hash (Hex)"] = $"0x{entity.Model.Hash:X}";
                list[""] = "";

                if (entity is Vehicle veh)
                {
                    list["Engine Health"] = $"{veh.Fx.EngineHealth:n1} / 1,000.0";
                    list["Body Health"] = $"{veh.Fx.BodyHealth:n1} / 1,000.0";
                    list["Speed"] = $"{veh.Fx.Speed / 0.621371f:n3} MP/H";
                    list["RPM"] = $"{veh.Fx.CurrentRPM:n3}";
                    list["Current Gear"] = $"{veh.Fx.CurrentGear}";
                    list["Acceleration"] = $"{veh.Fx.Acceleration:n3}";
                    list["Lock Status"] = $"{veh.Fx.LockStatus}";
                    list["Fuel"] = $"{API.DecorGetFloat(veh.Handle, "Vehicle.Fuel")}";

                    if (veh.Fx.Driver != null)
                    {
                        list["Driver"] = $"{veh.Fx.Driver.Handle}";
                        list["Driver Visible"] = $"{veh.Fx.Driver.IsVisible}";
                        list["Driver Opacity"] = $"{veh.Fx.Driver.Opacity}";
                    }
                }
                else if (entity is Ped ped)
                {
                    list["Health"] = $"{ped.Health} / {ped.MaxHealth}";

                    list["Important"] = $"{Decorators.GetBoolean(ped.Handle, Decorators.PED_IMPORTANT)}";
                    list["MissionPed"] = $"{Decorators.GetBoolean(ped.Handle, Decorators.PED_MISSION)}";
                    list["Hostage"] = $"{Decorators.GetBoolean(ped.Handle, Decorators.PED_HOSTAGE)}";
                    list["Arrested"] = $"{Decorators.GetBoolean(ped.Handle, Decorators.PED_ARREST)}";
                    list["Suspect"] = $"{Decorators.GetBoolean(ped.Handle, Decorators.PED_SUSPECT)}";
                    list["Arrestable"] = $"{Decorators.GetBoolean(ped.Handle, Decorators.PED_ARRESTABLE)}";

                    //if (ped.Fx.IsInGroup)
                    //{
                    //    list["GroupId"] = $"{ped.Fx.PedGroup.Handle}";
                    //    list["Player GroupID"] = $"{Game.PlayerPed.PedGroup.Handle}";
                    //}
                }
                else
                {
                    list["Health"] = $"{entity.Health} / {entity.MaxHealth}";
                }
                list[" "] = "";
                list["Distance"] = $"{Math.Sqrt(Game.PlayerPed.Position.DistanceToSquared(entity.Position)):n3} Meters";
                list["Heading"] = $"{entity.Heading:n3}";
                list["Position"] = $"{pos.X:n5} {pos.Y:n5} {pos.Z:n5}";
                list["Rotation"] = $"{rot.X:n5} {rot.Y:n5} {rot.Z:n5}";
                list["Velocity"] = $"{vel.X:n5} {vel.Y:n5} {vel.Z:n5}";
                list["Visible"] = $"{entity.IsVisible}";
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
            }
            return list;
        }

        public static Vector2 WorldToScreen(Vector3 position)
        {
            float screenX = 0f;
            float screenY = 0f;

            return API.World3dToScreen2d(position.X, position.Y, position.Z, ref screenX, ref screenY) ? Vector2.Zero : new Vector2(screenX, screenY);
        }

        public static void DrawRect(float xPos, float yPos, float xScale, float yScale, Color color)
        {
            try
            {
                API.DrawRect(xPos, yPos, xScale, yScale, color.R, color.G, color.B, color.A);
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
            }
        }

        public static void DrawText(string text, Vector2 pos, Color? color = null, float scale = 0.25f,
            bool shadow = false, int shadowOffset = 1, Alignment alignment = Alignment.Left, CitizenFX.Core.UI.Font font = CitizenFX.Core.UI.Font.ChaletLondon)
        {
            try
            {
                API.SetTextFont((int)font);
                API.SetTextProportional(false);
                API.SetTextScale(scale, scale);
                if (shadow)
                {
                    API.SetTextDropshadow(shadowOffset, 0, 0, 0, 255);
                }
                var col = color ?? DefaultColor;
                API.SetTextColour(col.R, col.G, col.B, col.A);
                API.SetTextEdge(1, 0, 0, 0, 255);
                API.SetTextJustification((int)alignment);
                API.SetTextEntry("STRING");
                API.AddTextComponentSubstringPlayerName(text);
                API.DrawText(pos.X, pos.Y);
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
            }
        }

        private static string GetModelName(Model model)
        {
            var name = "";
            if (model.IsVehicle)
            {
                name = Enum.GetName(typeof(VehicleHash), (VehicleHash)model.Hash) ?? "";
            }

            if (string.IsNullOrEmpty(name))
            {
                name = Enum.GetName(typeof(PedHash), (PedHash)model.Hash) ?? "Unknown";
            }
            return name;
        }
    }
}
