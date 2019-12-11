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
    class ChatPosition
    {
        public int x;
        public int y;
    }

    class ChatService
    {
        static Client client = Client.GetInstance();

        static bool PreviousChatboxState = false;
        static bool PlayerSpawned = false;

        static public void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Chat:Message", new Action<string>(OnChatMessage));

            RegisterNuiCallbackType("SendChatMessage");
            client.RegisterEventHandler("__cfx_nui:SendChatMessage", new Action<System.Dynamic.ExpandoObject>(HandleChatResult));

            RegisterNuiCallbackType("ChatPosition");
            client.RegisterEventHandler("__cfx_nui:ChatPosition", new Action<System.Dynamic.ExpandoObject>(StoreChatPosition));

            RegisterNuiCallbackType("CloseChatMessage");
            client.RegisterEventHandler("__cfx_nui:CloseChatMessage", new Action(OnNuiCloseChat));

            client.RegisterTickHandler(OnChatTask);

            client.RegisterEventHandler("playerSpawned", new Action(OnPlayerSpawned));
        }

        static void OnPlayerSpawned()
        {
            PlayerSpawned = true;

            int x = API.GetResourceKvpInt("CHAT_X");
            int y = API.GetResourceKvpInt("CHAT_Y");

            if (x == 0)
                x = 1920;

            ChatPosition chatPosition = new ChatPosition() { x = x, y = y };
            API.SendNuiMessage(JsonConvert.SerializeObject(chatPosition));
        }

        static void OnNuiCloseChat()
        {
            EnableChatbox(false);
        }

        static void OnCloseChat()
        {
            EnableControlAction(0, Control.CursorScrollUp, true);
            EnableControlAction(0, Control.CursorScrollDown, true);
            SetPedCanSwitchWeapon(Game.PlayerPed, true);
            API.SetNuiFocus(false, false);
            Client.TriggerEvent("curiosity:Client:Chat:ChatboxActive", false);
        }

        static void StoreChatPosition(System.Dynamic.ExpandoObject data)
        {
            try
            {
                IDictionary<string, object> result = data;

                string x = (string)result["x"];
                string y = (string)result["y"];

                API.SetResourceKvpInt("CHAT_X", int.Parse(x));
                API.SetResourceKvpInt("CHAT_Y", int.Parse(y));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"StoreChatPosition ERROR: {ex.Message}");
            }
        }

        static void OnChatMessage(string encodedMessage)
        {
            try
            {
                if (string.IsNullOrEmpty(encodedMessage)) return;

                string json = Encode.Base64ToString(encodedMessage);
                API.SendNuiMessage(json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}");
            }
        }

        static void HandleChatResult(System.Dynamic.ExpandoObject data)
        {
            try
            {
                EnableChatbox(false);

                API.SetNuiFocus(false, false);

                IDictionary<string, object> chatResult = data;

                if (!chatResult.ContainsKey("msg") || string.IsNullOrWhiteSpace((string)chatResult["msg"]))
                    return;

                string message = (String)chatResult["msg"];
                string chatChannel = (String)chatResult["chat"];

                var spaceSplit = message.Split(' ');

                if (message.Substring(0, 1) == "/" && message.Length >= 2)
                {
                    API.ExecuteCommand(message.Substring(1));
                }
                else
                {
                    Client.TriggerServerEvent("curiosity:Server:Chat:Message", message, chatChannel);
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
                else if (Game.IsControlJustPressed(0, Control.MpTextChatAll) || Game.IsControlJustReleased(0, Control.MpTextChatAll))
                {
                    if (PlayerSpawned)
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
                    chatState.showChat = state;
                    API.SendNuiMessage(JsonConvert.SerializeObject(chatState));

                    PreviousChatboxState = state;

                    if (PreviousChatboxState)
                    {
                        API.SetNuiFocus(true, true);
                        Client.TriggerEvent("curiosity:Client:Chat:ChatboxActive", true);
                    }
                    else
                    {
                        API.SetNuiFocus(false, false);
                        OnCloseChat();
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
