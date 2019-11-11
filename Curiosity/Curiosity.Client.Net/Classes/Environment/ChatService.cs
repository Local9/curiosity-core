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

        static public void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Chat:Message", new Action<string>(OnChatMessage));

            RegisterNuiCallbackType("SendChatMessage");
            client.RegisterEventHandler("__cfx_nui:SendChatMessage", new Action<System.Dynamic.ExpandoObject>(HandleChatResult));

            RegisterNuiCallbackType("ChatPosition");
            client.RegisterEventHandler("__cfx_nui:ChatPosition", new Action<System.Dynamic.ExpandoObject>(StoreChatPosition));

            RegisterNuiCallbackType("CloseChatMessage");
            client.RegisterEventHandler("__cfx_nui:CloseChatMessage", new Action(OnCloseChat));

            client.RegisterTickHandler(OnChatTask);

            client.RegisterEventHandler("playerSpawned", new Action(OnPlayerSpawned));
        }

        static void OnPlayerSpawned()
        {
            int x = API.GetResourceKvpInt("CHAT_X");
            int y = API.GetResourceKvpInt("CHAT_Y");

            if (x == 0)
                x = 1920;

            ChatPosition chatPosition = new ChatPosition() { x = x, y = y };
            API.SendNuiMessage(JsonConvert.SerializeObject(chatPosition));
        }

        static void OnCloseChat()
        {
            EnableControlAction(0, Control.CursorScrollUp, true);
            EnableControlAction(0, Control.CursorScrollDown, true);
            SetPedCanSwitchWeapon(Game.PlayerPed, true);
            SetNuiFocus(false);
            PreviousChatboxState = false;
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
                string json = Encode.Base64ToString(encodedMessage);
                API.SendNuiMessage(json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
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
