﻿using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using System.Drawing;
using System.Security.Cryptography;

namespace Curiosity.Core.Client.Utils
{
    public static class Common
    {

        public static void LoadMissingMapObjects()
        {
            ActivateInteriorEntitySets(new Vector3(-1155.31005f, -1518.5699f, 10.6300001f), "swap_clean_apt", "layer_whiskey", "layer_sextoys_a", "swap_mrJam_A", "swap_sofa_A"); // Floyd Apartment
            ActivateInteriorEntitySets(new Vector3(-802.31097f, 175.05599f, 72.84459f), "V_Michael_bed_tidy", "V_Michael_L_Items", "V_Michael_S_Items", "V_Michael_D_Items", "V_Michael_M_Items", "Michael_premier", "V_Michael_plane_ticket"); // Michael House
            ActivateInteriorEntitySets(new Vector3(-9.96562f, -1438.54003f, 31.101499f), "V_57_FranklinStuff"); // Franklin Aunt House
            ActivateInteriorEntitySets(new Vector3(0.91675f, 528.48498f, 174.628005f), "franklin_settled", "franklin_unpacking", "bong_and_wine", "progress_flyer", "progress_tshirt", "progress_tux", "unlocked"); // Franklin House

            // Stilts Apartment kitchen window
            ActivateInteriorEntitySets(new Vector3(-172.983001f, 494.032989f, 137.654006f), "Stilts_Kitchen_Window"); // 3655 Wild Oats
            ActivateInteriorEntitySets(new Vector3(340.941009f, 437.17999f, 149.389999f), "Stilts_Kitchen_Window"); // 2044 North Conker
            ActivateInteriorEntitySets(new Vector3(373.0230102f, 416.1050109f, 145.70100402f), "Stilts_Kitchen_Window");// 2045 North Conker
            ActivateInteriorEntitySets(new Vector3(-676.1270141f, 588.6119995f, 145.16999816f), "Stilts_Kitchen_Window"); // 2862 Hillcrest Avenue
            ActivateInteriorEntitySets(new Vector3(-763.10699462f, 615.90600585f, 144.139999f), "Stilts_Kitchen_Window"); // 2868 Hillcrest Avenue
            ActivateInteriorEntitySets(new Vector3(-857.79797363f, 682.56298828f, 152.6529998f), "Stilts_Kitchen_Window"); // 2874 Hillcrest Avenue
            ActivateInteriorEntitySets(new Vector3(-572.60998535f, 653.13000488f, 145.63000488f), "Stilts_Kitchen_Window"); // 2117 Milton Road
            ActivateInteriorEntitySets(new Vector3(120.5f, 549.952026367f, 184.09700012207f), "Stilts_Kitchen_Window"); // 3677 Whispymound Drive
            ActivateInteriorEntitySets(new Vector3(-1288f, 440.74798583f, 97.694602966f), "Stilts_Kitchen_Window"); // 2113 Mad Wayne Thunder Drive
        }

        public static void ActivateInteriorEntitySets(Vector3 position, params string[] entities)
        {
            int interior = GetInteriorAtCoords(position.X, position.Y, position.Z);
            foreach (string ent in entities)
            {
                ActivateInteriorEntitySet(interior, ent);
            }
            RefreshInterior(interior);
        }

        public static Dictionary<int, string> WorldCompassDirection = new()
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

        public static bool IsEntityInAngledArea(Entity entity, Vector3 start, Vector3 end, float width, bool setZStart = true, bool includeZ = true, bool debug = false)
        {
            if (entity == null) return false;
            if (start == Vector3.Zero) return false;
            if (end == Vector3.Zero) return false;

            bool isEntityInAngledArea = API.IsEntityInAngledArea(entity.Handle, start.X, start.Y, start.Z, end.X, end.Y, end.Z, width, false, includeZ, 0);

            if (debug)
            {
                Vector3 center = (start + end) / 2;
                if (setZStart) center.Z = start.Z;

                float rotation = GetHeading(start, end);

                string compassNative = GetCompassHeading((float)rotation);

                float distance = start.Distance(end);
                float height = (start.Z > end.Z) ? start.Z - end.Z : end.Z - start.Z;

                Color colorTest = Color.FromArgb(120, !isEntityInAngledArea ? 255 : 0, isEntityInAngledArea ? 255 : 0, 0);
                Color debugSphere = Color.FromArgb(80, 255, 77, 196);
                
                World.DrawMarker((MarkerType)43, center, Vector3.Zero, new Vector3(Vector2.Zero, (float)rotation), new Vector3(width, distance, height), colorTest);
                World.DrawMarker(MarkerType.DebugSphere, start, Vector3.Zero, Vector3.Zero, new Vector3(0.5f), debugSphere);
                World.DrawMarker(MarkerType.DebugSphere, end, Vector3.Zero, Vector3.Zero, new Vector3(0.5f), debugSphere);
                
                ScreenInterface.Draw3DText(start, $"START: {start}", 40f, distance + 10f, 0f);
                ScreenInterface.Draw3DText(end, $"END: {end}", 40f, distance + 10f, 0f);

                ScreenInterface.Draw3DText(center + new Vector3(0, 0, 0.9f), $"DISTANCE: {distance}", 40f, distance + 10f, 0f);
                ScreenInterface.Draw3DText(center + new Vector3(0, 0, 0.7f), $"HEADING: {rotation} / COMPASS: {compassNative}", 40f, distance + 10f, 0f);

                ScreenInterface.Draw3DText(center + new Vector3(0, 0, 0.5f), $"WIDTH: {width}", 40f, distance + 10f, 0f);
                ScreenInterface.Draw3DText(center + new Vector3(0, 0, 0.3f), $"HEIGHT: {height}", 40f, distance + 10f, 0f);
            }

            return isEntityInAngledArea;
        }

        public static string GetVehicleHeadingDirection()
        {
            if (!Game.PlayerPed.IsInVehicle()) return "U";

            foreach (KeyValuePair<int, string> kvp in WorldCompassDirection)
            {
                float vehDirection = Game.PlayerPed.CurrentVehicle.Heading;
                if (Math.Abs(vehDirection - kvp.Key) < 22.5)
                {
                    return kvp.Value;
                }
            }

            return "U";
        }

        public static string GetHeadingDirection()
        {
            foreach (KeyValuePair<int, string> kvp in WorldCompassDirection)
            {
                float vehDirection = Game.PlayerPed.Heading;
                if (Math.Abs(vehDirection - kvp.Key) < 22.5)
                {
                    return kvp.Value;
                }
            }

            return "U";
        }

        public static string GetCompassHeading(Vector3 start, Vector3 end)
        {
            float heading = GetHeading(start, end);

            foreach (KeyValuePair<int, string> kvp in WorldCompassDirection)
            {
                float vehDirection = heading;
                if (Math.Abs(vehDirection - kvp.Key) < 22.5)
                {
                    return kvp.Value;
                }
            }

            return "U";
        }

        public static string GetCompassHeading(float heading)
        {
            foreach (KeyValuePair<int, string> kvp in WorldCompassDirection)
            {
                float vehDirection = heading;
                if (Math.Abs(vehDirection - kvp.Key) < 22.5)
                {
                    return kvp.Value;
                }
            }

            return "U";
        }

        public static float GetHeading(Vector3 start, Vector3 end)
        {
            float dx = end.X - start.X;
            float dy = start.Y - end.Y;
            double val = (Math.Atan2(dx, dy) + Math.PI) * (180.0f / Math.PI);
            return (float)val;
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

        public static Vector3 GetRandomSpawnCoordsInRange(Vector3 center, float minRange, float maxRange, out float heading, bool findGroundZ = true)
        {
            heading = GetRandomFloatInRange(0f, 360f);
            var headingRad = heading * (Math.PI / 180f);
            var distance = GetRandomFloatInRange(minRange, maxRange);
            var offset = new Vector3(-(float)Math.Sin(headingRad), (float)Math.Cos(headingRad), 0) * distance;

            float groundZ = 0f;
            if (GetGroundZFor_3dCoord(center.X + offset.X, center.Y + offset.Y, center.Z, ref groundZ, false) && findGroundZ)
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

        public static bool IsPointWithinCircle(float circleRadius, float circleCenterPointX, float circleCenterPointY, float pointToCheckX, float pointToCheckY)
        {
            return (Math.Pow(pointToCheckX - circleCenterPointX, 2) + Math.Pow(pointToCheckY - circleCenterPointY, 2)) < (Math.Pow(circleRadius, 2));
        }

        public static bool IsPointWithinSphere(this Vector3 PointToCheck, float sphereRadius, Vector3 SphereCenter)
        {
            return (Math.Pow(SphereCenter.X - PointToCheck.X, 2) + Math.Pow(SphereCenter.Y - PointToCheck.Y, 2) + Math.Pow(SphereCenter.Z - PointToCheck.Z, 2)) < Math.Pow(sphereRadius, 2);
        }

        public static void RemoveIpls(List<string> ipls)
        {
            foreach (string ipl in ipls)
            {
                Logger.Debug($"Remove IPL: {ipl}");
                if (IsIplActive(ipl)) RemoveIpl(ipl);
            }
        }

        public static void RequestIpls(List<string> ipls)
        {
            foreach (string ipl in ipls)
            {
                Logger.Debug($"Request IPL: {ipl}");
                if (!IsIplActive(ipl)) RequestIpl(ipl);
            }
        }

        public static float Map(float x, float inMin, float inMax, float outMin, float outMax, bool clamp = false)
        {
            float val = (float)((x - inMin) * (outMax - outMin) / (inMax - inMin)) + outMin;
            if (clamp)
                val = Clamp(val, outMin, outMax);
            return val;
        }

        public static float Clamp(float val, float min, float max)
        {
            if (val.CompareTo(min) < 0)
                return min;
            return val.CompareTo(max) > 0 ? max : val;
        }

        public static async void StartScreenFx(string name, int duration, bool looped, bool stopAfterDuration)
        {
            AnimpostfxPlay(name, duration, looped);
            await BaseScript.Delay(duration);
            if (stopAfterDuration)
                StopScreenFx(name);
        }

        public static void StopScreenFx(string name)
        {
            AnimpostfxStop(name);
        }
    }
}
