using System;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Client.Net.Helper
{
    /// <summary>
    /// Sometimes you may want to use this, sometimes not
    /// It is helpful for keeping track of what the arguments are to a particular native
    /// you use a lot
    /// </summary>
    public static class NativeWrappers
    {
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

        static public void DrawHelpText(string message)
        {
            SetTextComponentFormat("STRING");
            AddTextComponentString(message);
            DisplayHelpTextFromStringLabel(0, false, true, -1);
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
    }
}
