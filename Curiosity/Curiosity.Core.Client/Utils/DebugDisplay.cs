using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Extensions;
using Curiosity.Systems.Library.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Curiosity.Core.Client.Utils
{
    static class DebugDisplay
    {
        private static readonly Vector2 DefaultPos = new Vector2(0.6f, 0.5f);
        public static readonly Color DefaultColor = Color.FromArgb(255, 255, 255);
        const float CONVERT_SPEED_MPH = 2.236936f;
        const float CONVERT_SPEED_KPH = 3.6f;

        public static void DrawData(this Entity entity)
        {
            var entityPos = entity.Position;

            if (entityPos.Distance(Cache.PlayerPed.Position) > 20f) return;

            var pos = WorldToScreen(entityPos);
            if (pos.X <= 0f || pos.Y <= 0f || pos.X >= 1f || pos.Y >= 1f)
            {
                pos = DefaultPos;
            }
            var dist = (float)Math.Sqrt(Cache.PlayerPed.Position.DistanceToSquared(entityPos));
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
                list["Created By"] = $"{entity.State.Get(StateBagKey.CURIOSITY_CREATED) ?? "Unknown"}";
                list[""] = "";

                if (entity is Vehicle veh)
                {
                    bool hasState = veh.State.Get($"{StateBagKey.VEH_SPAWNED}") ?? false;
                    if (hasState)
                    {
                        float spd = veh.Speed;

                        list["Server Spawned"] = $"{veh.State.Get(StateBagKey.VEH_SPAWNED) ?? false}";
                        list["Owner"] = $"[{veh.State.Get(StateBagKey.VEH_OWNER_ID)}] {veh.State.Get($"{StateBagKey.VEH_OWNER}")}";
                        list["Spawn Type"] = $"{veh.State.Get(StateBagKey.VEH_SPAWN_TYPE) ?? "Unknown"}";
                        list["Class"] = $"{veh.ClassType}";
                        list["Personal"] = $"{veh.State.Get(StateBagKey.VEH_PERSONAL) ?? false}";
                        list["Personal Trailer"] = $"{veh.State.Get(StateBagKey.VEH_PERSONAL_TRAILER) ?? false}";
                        list["Lock State"] = $"{veh.LockStatus}";
                        list["Exclusive"] = $"{API.IsPedExclusiveDriverOfVehicle(Game.PlayerPed.Handle, veh.Handle, 0)}";
                        list["Speed (MPH)"] = $"{spd * CONVERT_SPEED_MPH}";
                        list["Speed (KPH)"] = $"{spd * CONVERT_SPEED_KPH}";

                        list["-"] = "";
                        
                        if (veh.State.Get($"{StateBagKey.VEH_FUEL}") != null)
                        {
                            list["Fuel"] = $"{veh.State.Get(StateBagKey.VEH_FUEL) ?? 0}";
                            list["Fuel Multiplier"] = $"{veh.State.Get(StateBagKey.VEH_FUEL_MULTIPLIER) ?? 0}";
                        }
                    }

                    list["--"] = "";

                    if (veh.Driver.Exists())
                    {
                        list["Driver"] = $"{veh.Driver.Handle}";
                        list["Driver Visible"] = $"{veh.Driver.IsVisible}";
                        list["Driver Opacity"] = $"{veh.Driver.Opacity}";
                    }
                }
                else if (entity is Ped ped)
                {
                    list["Health"] = $"{ped.Health} / {ped.MaxHealth}";

                    if (ped.IsInGroup)
                    {
                        list["GroupId"] = $"{ped.PedGroup.Handle}";
                        list["Player GroupID"] = $"{Cache.PlayerPed.PedGroup.Handle}";
                    }
                }

                list["Health"] = $"{entity.Health} / {entity.MaxHealth}";
                list["---"] = "";
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
