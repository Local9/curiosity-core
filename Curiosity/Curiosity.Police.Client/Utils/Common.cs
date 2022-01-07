using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Curiosity.Police.Client.Extensions;
using System.Drawing;
using CitizenFX.Core.Native;
using Curiosity.Police.Client.Interface;

namespace Curiosity.Police.Client.Utils
{
    public static class Common
    {
        static Dictionary<int, string> _worldDirection = new()
        {
            { 0, "N" },
            { 45, "NW" },
            { 90, "W" },
            { 135, "SW" },
            { 180, "S" },
            { 225, "SE" },
            { 270, "E" },
            { 315, "NE" },
            { 360, "N" }
        };

        public static bool IsEntityInAngledArea(Entity entity, Vector3 start, Vector3 end, float width, bool setZStart = true, bool debug = false)
        {
            if (entity == null) return false;
            if (start == Vector3.Zero) return false;
            if (end == Vector3.Zero) return false;

            bool isEntityInAngledArea = API.IsEntityInAngledArea(entity.Handle, start.X, start.Y, start.Z, end.X, end.Y, end.Z, width, false, false, 0);

            if (debug)
            {
                Vector3 center = (start + end) / 2;
                if (setZStart) center.Z = start.Z;

                float rotation = GetHeading(start, end);
                float distance = start.Distance(end);
                float height = (start.Z > end.Z) ? start.Z - end.Z : end.Z - start.Z;

                Color colorTest = Color.FromArgb(120, !isEntityInAngledArea ? 255 : 0, isEntityInAngledArea ? 255 : 0, 0);
                Color debugSphere = Color.FromArgb(80, 255, 77, 196);
                World.DrawMarker((MarkerType)43, center, Vector3.Zero, new Vector3(0f, 0f, rotation), new Vector3(width, distance, height), colorTest);
                World.DrawMarker(MarkerType.DebugSphere, start, Vector3.Zero, Vector3.Zero, new Vector3(0.5f), debugSphere);
                World.DrawMarker(MarkerType.DebugSphere, end, Vector3.Zero, Vector3.Zero, new Vector3(0.5f), debugSphere);
                ScreenInterface.Draw3DText(start, $"START: {start}", 40f, distance + 10f, 0f);
                ScreenInterface.Draw3DText(end, $"END: {end}", 40f, distance + 10f, 0f);
            }

            return isEntityInAngledArea;
        }

        public static string GetHeadingDirection()
        {
            foreach (KeyValuePair<int, string> kvp in _worldDirection)
            {
                float vehDirection = Game.PlayerPed.Heading;
                if (Math.Abs(vehDirection - kvp.Key) < 22.5)
                {
                    return kvp.Value;
                }
            }

            return "U";
        }

        public static float GetHeading(Vector3 start, Vector3 end)
        {
            Vector2 eStart = new Vector2(start.X, start.Y);
            Vector2 eEnd = new Vector2(end.X, end.Y);
            return GetHeading(eStart, eEnd);
        }

        public static float GetHeading(Vector2 start, Vector2 end)
        {
            float dx = start.X - end.X;
            float dy = start.Y - end.Y;
            return GetHeadingFromVector_2d(dx, dy);
        }

        public static void Notification(string message, bool blink = false, bool saveToBrief = false)
        {
            SetNotificationTextEntry("CELL_EMAIL_BCON");
            foreach (string s in CitizenFX.Core.UI.Screen.StringToArray(message))
            {
                AddTextComponentSubstringPlayerName(s);
            }
            DrawNotification(blink, saveToBrief);
        }

        // source: https://github.com/TomGrobbe/vMenu/blob/master/vMenu/CommonFunctions.cs
        public static async Task<string> GetUserInput(string windowTitle, string defaultText, int maxInputLength)
        {
            // Create the window title string.
            var spacer = "\t";
            AddTextEntry($"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", $"{windowTitle ?? "Enter"}:{spacer}(MAX {maxInputLength.ToString()} Characters)");

            // Display the input box.
            DisplayOnscreenKeyboard(1, $"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", "", defaultText ?? "", "", "", "", maxInputLength);
            await BaseScript.Delay(0);
            // Wait for a result.
            while (true)
            {
                int keyboardStatus = UpdateOnscreenKeyboard();

                switch (keyboardStatus)
                {
                    case 3: // not displaying input field anymore somehow
                    case 2: // cancelled
                        return null;
                    case 1: // finished editing
                        return GetOnscreenKeyboardResult();
                    default:
                        await BaseScript.Delay(0);
                        break;
                }
            }
        }

        public static async Task RequestModel(uint model)
        {
            if (!IsModelValid(model))
                return;

            var start = DateTime.Now;

            while (!HasModelLoaded(model))
            {
                RequestModel(model);
                await BaseScript.Delay(10);
            }
        }

        public static async Task RequestCollision(uint model)
        {
            if (!IsModelValid(model))
                return;

            var start = DateTime.Now;
            var timeout = start + TimeSpan.FromSeconds(0.5);

            while (!HasCollisionForModelLoaded(model) && DateTime.Now < timeout)
            {
                RequestCollisionForModel(model);
                await BaseScript.Delay(10);
            }
        }

        public static async Task RequestPtfxAsset(string name)
        {
            var start = DateTime.Now;

            while (!HasNamedPtfxAssetLoaded(name))
            {
                RequestNamedPtfxAsset(name);
                await BaseScript.Delay(10);
            }
        }

        public static async Task NetworkRequestControl(int entity, int timeoutSeconds = 1)
        {
            if (!DoesEntityExist(entity))
                return;

            var start = DateTime.Now;
            var timeout = start + TimeSpan.FromSeconds(timeoutSeconds);

            while (!NetworkHasControlOfEntity(entity) && DateTime.Now < timeout)
            {
                NetworkRequestControlOfEntity(entity);
                await BaseScript.Delay(100);
            }
        }

        public static int GetPlayerPedOrVehicle()
        {
            var player = GetPlayerPed(-1);
            return IsPedInAnyVehicle(player, false) ? GetVehiclePedIsIn(player, false) : player;
        }

        public static bool IsPlayerAiming()
        {
            return IsPlayerFreeAiming(PlayerId()) || IsControlPressed(0, 25); // INPUT_AIM
        }

        public static bool GetClosestEntity(IEnumerable<int> entities, out int closest)
        {
            closest = -1;
            bool found = false;
            float minDist = float.MaxValue;
            var coords = GetEntityCoords(GetPlayerPed(-1), false);

            foreach (var entity in entities)
            {
                var pos = GetEntityCoords(entity, IsEntityAPed(entity));
                var dist = coords.DistanceToSquared(pos);

                if (dist < minDist)
                {
                    closest = entity;
                    minDist = dist;
                    found = true;
                }
            }

            return found;
        }

        public static void GetEntityMinMaxZ(int entity, out float minZ, out float maxZ)
        {
            var model = (uint)GetEntityModel(entity);
            var min = Vector3.Zero;
            var max = Vector3.Zero;
            GetModelDimensions(model, ref min, ref max);

            minZ = min.Z;
            maxZ = max.Z;
        }

        public static float GetEntityHeight(int entity)
        {
            GetEntityMinMaxZ(entity, out float minZ, out float maxZ);
            return maxZ - minZ;
        }

        public static float GetEntityHeightAboveGround(int entity)
        {
            var coords = GetEntityCoords(entity, false);
            float wheight = 0f;

            if (GetWaterHeightNoWaves(coords.X, coords.Y, coords.Z, ref wheight))
                return coords.Z - wheight;
            else
                return GetEntityHeightAboveGround(entity);
        }

        public static Vector3 GetEntityTopCoords(int entity)
        {
            var coords = GetEntityCoords(entity, IsEntityAPed(entity));
            GetEntityMinMaxZ(entity, out float minZ, out float maxZ);
            coords.Z += maxZ;
            return coords;
        }

        public static void ApplyTorque(int entity, float pitch, float roll, float yaw, bool scaleLeverage = true)
        {
            var min = -Vector3.One;
            var max = Vector3.One;

            if (scaleLeverage)
            {
                var model = (uint)GetEntityModel(entity);
                var isHeli = IsThisModelAHeli(model);
                GetModelDimensions(model, ref min, ref max);
            }

            if (pitch != 0)
            {
                ApplyForceToEntity(entity, 1, 0f, 0f, pitch, 0f, max.Y, 0f, -1, true, true, true, false, false);
                ApplyForceToEntity(entity, 1, 0f, 0f, -pitch, 0f, min.Y, 0f, -1, true, true, true, false, false);
            }

            if (roll != 0)
            {
                ApplyForceToEntity(entity, 1, 0f, 0f, roll, max.X, 0f, 0f, -1, true, true, true, false, false);
                ApplyForceToEntity(entity, 1, 0f, 0f, -roll, min.X, 0f, 0f, -1, true, true, true, false, false);
            }

            if (yaw != 0)
            {
                ApplyForceToEntity(entity, 1, 0f, yaw, 0f, max.X, 0f, 0f, -1, true, true, true, false, false);
                ApplyForceToEntity(entity, 1, 0f, -yaw, 0f, min.X, 0f, 0f, -1, true, true, true, false, false);
            }
        }

        public static bool EnsurePlayerIsInVehicle(out int player, out int vehicle, bool notification = true)
        {
            vehicle = 0;
            player = GetPlayerPed(-1);
            if (!IsPedInAnyVehicle(player, true))
            {
                if (notification)
                    Notification("Player is not in a vehicle");

                return false;
            }

            vehicle = GetVehiclePedIsIn(player, false);
            return true;
        }

        public static bool EnsurePlayerIsVehicleDriver(out int player, out int vehicle, bool notification = true)
        {
            if (!EnsurePlayerIsInVehicle(out player, out vehicle, notification))
                return false;

            var driver = GetPedInVehicleSeat(vehicle, -1);

            if (driver != player)
            {
                if (notification)
                    Notification("Player is not the driver of this vehicle");

                return false;
            }

            return true;
        }

        public static bool GetWaypoint(out Vector3 wp, bool adjust = true)
        {
            wp = Vector3.Zero;

            if (!IsWaypointActive())
                return false;

            wp = GetBlipInfoIdCoord(GetFirstBlipInfoId(8));

            if (adjust)
            {
                var adjustedWp = Vector3.Zero;
                if (GetClosestVehicleNode(wp.X, wp.Y, wp.Z, ref adjustedWp, 1, 100f, 2.5f))
                    wp = adjustedWp;
            }

            return true;
        }

        public static List<int> GetObjects()
        {
            var objs = new List<int>();
            int obj = 0;
            int handle = FindFirstObject(ref obj);
            var coords = GetEntityCoords(GetPlayerPed(-1), true);

            if (handle == -1)
                return objs;

            do
            {
                objs.Add(obj);

            } while (FindNextObject(handle, ref obj));

            EndFindObject(handle);
            return objs;
        }

        public static void GetAimCoords(out Vector3 position, out Vector3 target, float distance)
        {
            position = GetGameplayCamCoords();
            var rot = GetGameplayCamRot(2);
            var forward = RotationToDirection(rot) * distance;
            target = position + forward;
        }

        public static void GetCamHorizontalForwardAndRightVectors(out Vector3 forward, out Vector3 right)
        {
            var heading = GetGameplayCamRot(2).Z;
            var headingRad = heading * (Math.PI / 180f);
            forward = new Vector3(-(float)Math.Sin(headingRad), (float)Math.Cos(headingRad), 0f);
            right = new Vector3(forward.Y, -forward.X, 0f);
        }

        public static Vector3 RotationToDirection(Vector3 rot)
        {
            float radiansZ = rot.Z * 0.0174532924f;
            float radiansX = rot.X * 0.0174532924f;
            float num = Math.Abs((float)Math.Cos(radiansX));
            return new Vector3
            {
                X = -(float)Math.Sin(radiansZ) * num,
                Y = (float)Math.Cos(radiansZ) * num,
                Z = (float)Math.Sin(radiansX)
            };
        }

        public static Vector3 DirectionToRotation(Vector3 dir)
        {
            dir.Normalize();
            var pitch = Math.Asin(-dir.Z) * 57.2957795;
            var yaw = Math.Atan2(dir.X, -dir.Y) * 57.2957795;
            return new Vector3((float)pitch, 0f, (float)yaw);
        }

        public static float GetAngleDifference(float a, float b, bool deg = true)
        {
            var pi = deg ? 180f : (float)Math.PI;
            //return pi - Math.Abs(Math.Abs(a - b) - pi);

            var diff = a - b;
            return diff > pi ? diff - (2 * pi) : diff;
        }

        public static Vector3 GetRandomSpawnCoordsInRange(Vector3 center, float minRange, float maxRange, out float heading)
        {
            heading = GetRandomFloatInRange(0f, 360f);
            var headingRad = heading * (Math.PI / 180f);
            var distance = GetRandomFloatInRange(minRange, maxRange);
            var offset = new Vector3(-(float)Math.Sin(headingRad), (float)Math.Cos(headingRad), 0) * distance;

            float groundZ = 0f;
            if (GetGroundZFor_3dCoord(center.X + offset.X, center.Y + offset.Y, center.Z, ref groundZ, false))
                offset.Z = groundZ - center.Z;

            return center + offset;
        }

        public static bool IsNamedRenderTargetRegistered(string target)
        {
            if (!string.IsNullOrEmpty(target))
            {
                if (!IsNamedRendertargetRegistered(target))
                {
                    RegisterNamedRendertarget(target, false);
                }
                return true;
            }
            return false;
        }

        public static bool LinkNamedRenderTarget(uint target)
        {
            if (target != 0)
            {
                if (!IsNamedRendertargetLinked(target))
                {
                    LinkNamedRendertarget(target);
                }
                return true;
            }
            return false;
        }

        public static bool IsNamedAndLinkedTargetRegistered(string target, uint hash)
        {
            return IsNamedRenderTargetRegistered(target) && LinkNamedRenderTarget(hash);
        }

        public static int GetNamedRenderTargetRenderId(string target)
        {
            if (!string.IsNullOrEmpty(target))
            {
                return GetNamedRendertargetRenderId(target);
            }
            return -1;
        }
    }
}
