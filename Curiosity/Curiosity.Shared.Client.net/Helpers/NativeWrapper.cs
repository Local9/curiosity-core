using System;
using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Shared.Client.net.Helper
{
    /// <summary>
    /// Sometimes you may want to use this, sometimes not
    /// It is helpful for keeping track of what the arguments are to a particular native
    /// you use a lot
    /// </summary>
    public static class NativeWrappers
    {
        public static bool TimeoutStateValue = false;

        public static void DrawHelpText(string message)
        {
            SetTextComponentFormat("STRING");
            AddTextComponentString(message);
            DisplayHelpTextFromStringLabel(0, false, true, -1);
        }

        public static bool NetworkIsSessionStarted()
        {
            return Function.Call<bool>(Hash.NETWORK_IS_SESSION_STARTED, new InputArgument[0]);
        }

        public static int PlayerId()
        {
            return Function.Call<int>(Hash.PLAYER_ID);
        }

        public static bool IsPedFatallyInjured(int ped)
        {
            return Function.Call<bool>(Hash.IS_PED_FATALLY_INJURED, ped);
        }

        public static void ClearPedTasksImmediately(int ped)
        {
            Function.Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, ped);
        }

        public static void RequestCollisionAtCoord(Vector3 location)
        {
            Function.Call(Hash.REQUEST_COLLISION_AT_COORD, location.X, location.Y, location.Z);
        }

        public static void LoadScene(Vector3 location)
        {
            Function.Call(Hash.LOAD_SCENE, location.X, location.Y, location.Z);
        }

        public static void NetworkResurrectLocalPlayer(Vector3 location, float heading)
        {
            Function.Call(Hash.NETWORK_RESURRECT_LOCAL_PLAYER, location.X, location.Y, location.Z, heading, true, true, false);
        }

        public static void SetEntityCoordsNoOffset(int handle, Vector3 location)
        {
            Function.Call(Hash.SET_ENTITY_COORDS_NO_OFFSET, handle, location.X, location.Y, location.Z, false, false, false, true);
        }

        public static void ClearPlayerWantedLevel(int playerId)
        {
            Function.Call(Hash.CLEAR_PLAYER_WANTED_LEVEL, playerId);
        }

        public static void ShutdownLoadingScreen()
        {
            Function.Call(Hash.SHUTDOWN_LOADING_SCREEN, new InputArgument[0]);
        }

        public static bool HasCollisionLoadedAroundEntity(int ped)
        {
            return Function.Call<bool>(Hash.HAS_COLLISION_LOADED_AROUND_ENTITY, ped);
        }

        public static string GetPlayerFromServerId(int playerId)
        {
            return Function.Call<string>(Hash.GET_PLAYER_FROM_SERVER_ID, playerId);
        }

        public static string GetPlayerName(string netId)
        {
            return Function.Call<string>(Hash.GET_PLAYER_NAME, netId);
        }

        public static bool NetworkIsPlayerActive(int playerId)
        {
            return Function.Call<bool>(Hash.NETWORK_IS_PLAYER_ACTIVE, playerId);
        }

        static public void RegisterNuiCallbackType(string TriggerName)
        {
            Function.Call(Hash.REGISTER_NUI_CALLBACK_TYPE, TriggerName);
        }

        static public void SetNuiFocus(bool toggle, bool nativeCursor = false)
        {
            Function.Call(Hash.SET_NUI_FOCUS, toggle, nativeCursor);
        }

        static public void EnableControlAction(int inputGroup, Control control, bool enable)
        {
            Function.Call(Hash.ENABLE_CONTROL_ACTION, inputGroup, control, enable);
        }

        static public void DisableControlAction(int inputGroup, Control control, bool disable)
        {
            Function.Call(Hash.DISABLE_CONTROL_ACTION, inputGroup, control, disable);
        }

        static public void SetPedCanSwitchWeapon(CitizenFX.Core.Ped ped, bool toggle)
        {
            Function.Call(Hash.SET_PED_CAN_SWITCH_WEAPON, ped, toggle);
        }

        static public void SendNuiMessage(string message)
        {
            Function.Call(Hash.SEND_NUI_MESSAGE, message);
        }

        static public void SetTextChatEnabled(bool toggle)
        {
            Function.Call(Hash.SET_TEXT_CHAT_ENABLED, toggle);
        }

        static public void CreateVehicle(string modelHash, float x, float y, float z, float heading, bool isNetwork = true, bool unknownParameter = true)
        {
            Function.Call(Hash.CREATE_VEHICLE, modelHash, x, y, z, heading, isNetwork, unknownParameter);
        }

        static public bool IsInputDisabled(int inputGroup)
        {
            return Function.Call<bool>(Hash._IS_INPUT_DISABLED, inputGroup);
        }

        static public bool NetworkIsPlayerTalking(Player player)
        {
            return Function.Call<bool>(Hash.NETWORK_IS_PLAYER_TALKING, player.Handle);
        }

        static public void SetObjectPhysicsParams(Entity obj, float mass, float gravity, float dragA, float dragB, float dragC, float rotDragA, float rotDragB, float rotDragC, float Unk, float MaxRotVel, float density)
        {
            Function.Call(Hash.SET_OBJECT_PHYSICS_PARAMS, obj.Handle, mass, gravity, dragA, dragB, dragC, rotDragA, rotDragB, rotDragC, Unk, MaxRotVel, density);
        }

        static public Vector3 GetEntityCords(Entity entity)
        {
            return Function.Call<Vector3>(Hash.GET_ENTITY_COORDS, entity);
        }

        public static bool EntityActive(int entityId)
        {
            if (!API.DoesEntityExist(entityId))
            {
                return false;
            }
            return !API.IsEntityDead(entityId);
        }

        static public async void Draw3DTextTimeout(float x, float y, float z, string message, int timeout)
        {
            TimeoutState(timeout);
            while (TimeoutStateValue)
            {
                await BaseScript.Delay(0);
                Draw3DText(x, y, z, message);
            }
        }

        static public async void TimeoutState(int timeout)
        {
            TimeoutStateValue = true;
            await BaseScript.Delay(timeout);
            TimeoutStateValue = false;
        }

        static public void Draw3DText(float x, float y, float z, string message)
        {
            float distance = (float)Math.Sqrt(GameplayCamera.Position.DistanceToSquared(new Vector3(x, y, z)));
            float scale = ((1 / distance) * 2) * GameplayCamera.FieldOfView / 20.0f;

            if (distance > 20.0f)
            {
                return;
            }

            SetTextScale(0.0f * scale, 1.1f * scale);
            SetTextFont(0);
            SetTextProportional(true);
            SetTextColour(255, 255, 255, 255);
            SetTextDropshadow(0, 0, 0, 0, 255);
            SetTextEdge(2, 0, 0, 0, 150);
            SetTextDropShadow();
            SetTextOutline();

            SetDrawOrigin(x, y, z + 1, 0);

            SetTextEntry("STRING");
            SetTextCentre(true);
            AddTextComponentString(message);

            EndTextCommandDisplayText(0, 0);
            ClearDrawOrigin();
        }

        static public Vector3 GetPositionInFrontOfEntity(Entity entity, float distance)
        {
            OutputArgument outputArgument = new OutputArgument();
            return Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_IN_WORLD_COORDS, entity, 0.0f, distance, 0.0);
        }

        static public float GetDistanceBetween(Vector3 start, Vector3 end, bool useZ = false)
        {
            return API.GetDistanceBetweenCoords(start.X, start.Y, start.Z, end.X, end.Y, end.Z, useZ);
        }

        public static bool IsWithinRange(Vector3 pos1, Vector3 pos2, float range)
        {
            // From Kronus
            // Check if it is within the cube of eachother.
            if (pos1.X - pos2.X > range || pos1.Y - pos2.Y > range
                || pos2.X - pos1.X > range || pos2.Y - pos1.Y > range
                || pos1.Z - pos2.Z > range || pos2.Z - pos1.Z > range) // Check Z last since it usually isn't the determining factor.
            {
                return false;
            }
            float distX = pos1.X - pos2.X;
            float distY = pos1.Y - pos2.Y;
            float distZ = pos1.Z - pos2.Z;
            distX *= distX;
            distY *= distY;
            distZ *= distZ;
            return distX + distY + distZ <= range * range;
        }

        //public static int NearestItemInCone(Vector3 loc, Vector3 dir, float range, float angle, Dictionary<int, LocationData> items)
        //{
        //    int currentClosestItem = -1; // Null item.
        //    float invSquaredRange = 1.0f / range;
        //    invSquaredRange *= invSquaredRange;
        //    float maxSquaredRatio = (float)Math.Sin(angle) * range;
        //    maxSquaredRatio *= maxSquaredRatio;
        //    float currentSquaredRadialDistance = maxSquaredRatio; // Maximum distance horizontally in the cone.
        //    maxSquaredRatio *= invSquaredRange;
        //    float currentLinearDistance = range; // Maximum vertical distance in the cone.
        //    float squaredRadialDistance;
        //    float linearDistance;
        //    float squaredLinearDistance;
        //    float x, y, z;
        //    float dot;
        //    float squaredTotalDistance;
        //    Vector3 target;
        //    foreach (int key in items.Keys)
        //    {
        //        target = items[key].Location; // Location of the target item.
        //        // Distances between each coordinate.
        //        x = target.X - loc.X;
        //        y = target.Y - loc.Y;
        //        z = target.Z - loc.Z;
        //        if (x > range || -x > range || y > range || -y > range || z > range || -z > range) // Not within the cube of detection, skip item.
        //        {
        //            continue;
        //        }

        //        squaredTotalDistance = x * x + y * y + z * z; // Total distance squared.
        //        if (squaredTotalDistance > range * range) // If it's too far away, skip item.
        //        {
        //            continue;
        //        }

        //        // Vector geometry. Basically Vel'Koz.
        //        dot = x * dir.X + y * dir.Y + z * dir.Z;
        //        linearDistance = dot / (dir.X * dir.X + dir.Y * dir.Y + dir.Z * dir.Z);
        //        squaredLinearDistance = linearDistance * linearDistance;
        //        squaredRadialDistance = squaredTotalDistance - squaredLinearDistance; // Piddagorean magic.
        //        //Log(string.Format("Linear distance = {0}, squaredRadialDistance = {1}, totalDistanceSquared = {2}, xyz = {3}, {4}, {5}", linearDistance, squaredRadialDistance, squaredTotalDistance, x, y, z));
        //        if (linearDistance < 0 || squaredRadialDistance / squaredLinearDistance > maxSquaredRatio) // Ensure it is within the cone.
        //        {
        //            continue;
        //        }

        //        if (ConeCompare(linearDistance, squaredRadialDistance, currentLinearDistance, currentSquaredRadialDistance))
        //        {
        //            currentLinearDistance = linearDistance;
        //            currentSquaredRadialDistance = squaredRadialDistance;
        //            currentClosestItem = key;
        //        }
        //    }
        //    return currentClosestItem;
        //}
    }
}
