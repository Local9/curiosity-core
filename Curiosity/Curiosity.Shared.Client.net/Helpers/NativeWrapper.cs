using System;
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

        static public Vector3 GetInFrontOfEntity(Entity entity, float distance)
        {
            OutputArgument outputArgument = new OutputArgument();
            return Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_IN_WORLD_COORDS, entity, 0.0f, distance, 0.0);
        }

        static public float GetDistanceBetween(Vector3 start, Vector3 end, bool useZ = false)
        {
            return API.GetDistanceBetweenCoords(start.X, start.Y, start.Z, end.X, end.Y, end.Z, useZ);
        }
    }
}
