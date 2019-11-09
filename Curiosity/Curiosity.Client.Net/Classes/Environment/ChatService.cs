using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static Curiosity.Shared.Client.net.Helper.NativeWrappers;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Global.Shared.net;
using Newtonsoft.Json;

namespace Curiosity.Client.net.Classes.Environment
{
    class ChatService
    {
        static Client client = Client.GetInstance();

        static bool PreviousChatboxState = false;

        static public void Init()
        {

            client.RegisterEventHandler("curiosity:Client:Chat:Message", new Action<string>(OnChatMessage));

            RegisterNuiCallbackType("SendChatMessage");
            client.RegisterEventHandler("__cfx_nui:SendChatMessage", new Action<System.Dynamic.ExpandoObject>(HandleChatResult));

            RegisterNuiCallbackType("CloseChatMessage");
            client.RegisterEventHandler("__cfx_nui:CloseChatMessage", new Action(OnCloseChat));

            client.RegisterTickHandler(OnChatTask);
        }

        static void OnCloseChat()
        {
            EnableControlAction(0, Control.CursorScrollUp, true);
            EnableControlAction(0, Control.CursorScrollDown, true);
            SetPedCanSwitchWeapon(Game.PlayerPed, true);
            SetNuiFocus(false);
            PreviousChatboxState = false;
        }

        static void OnChatMessage(string encodedMessage)
        {
            string json = Encode.Base64ToString(encodedMessage);
            API.SendNuiMessage(json);
        }

        static void HandleChatResult(System.Dynamic.ExpandoObject data)
        {
            try
            {
                EnableControlAction(0, Control.CursorScrollUp, true);
                EnableControlAction(0, Control.CursorScrollDown, true);
                SetPedCanSwitchWeapon(Game.PlayerPed, true);
                SetNuiFocus(false);
                PreviousChatboxState = false;

                IDictionary<string, object> chatResult = data;

                if (!chatResult.ContainsKey("msg") || string.IsNullOrWhiteSpace((string)chatResult["msg"]))
                    return;

                string message = (String)chatResult["msg"];

                var spaceSplit = message.Split(' ');

                if (message.Substring(0, 1) == "/" && message.Length >= 2)
                {
                    API.ExecuteCommand(message.Substring(1));
                }
                else
                {
                    Client.TriggerServerEvent("curiosity:Server:Chat:Message", message);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HandleChatResult ERROR: ${ex.Message}");
            }
        }

        static async Task OnChatTask()
        {
            try
            {
                SetTextChatEnabled(false);

                await Client.Delay(0);

                if (Game.IsControlPressed(0, Control.FrontendCancel))
                {
                    EnableChatbox(false);
                }

                if (Game.IsControlPressed(0, Control.MpTextChatAll))
                {
                    EnableChatbox(true);
                }

                if (PreviousChatboxState)
                {
                    DisableControlAction(0, Control.CursorScrollUp, true);
                    DisableControlAction(0, Control.CursorScrollDown, true);
                    SetPedCanSwitchWeapon(Game.PlayerPed, false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnChatTask ERROR: ${ex.Message}");
                EnableChatbox(false);
            }
        }

        static void EnableChatbox(bool state)
        {
            try
            {
                if (PreviousChatboxState != state)
                {
                    ChatState chatState = new ChatState();
                    chatState.display = state;
                    API.SendNuiMessage(JsonConvert.SerializeObject(chatState));

                    PreviousChatboxState = state;

                    if (PreviousChatboxState)
                    {
                        SetNuiFocus(true, true);
                    }
                    else
                    {
                        EnableControlAction(0, Control.CursorScrollUp, true);
                        EnableControlAction(0, Control.CursorScrollDown, true);
                        SetPedCanSwitchWeapon(Game.PlayerPed, true);

                        SetNuiFocus(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EnableChatbox ERROR: ${ex.Message}");
                EnableChatbox(false);
            }
        }
    }
}
