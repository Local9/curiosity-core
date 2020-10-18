using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.Entity;
using Curiosity.Shared.Client.net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Curiosity.Shared.Client.net.Helper.NativeWrappers;

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

            client.RegisterNuiEventHandler("SendChatMessage", new Action<IDictionary<string, object>, CallbackDelegate>(HandleChatResult));
            client.RegisterNuiEventHandler("ChatPosition", new Action<IDictionary<string, object>, CallbackDelegate>(StoreChatPosition));
            client.RegisterNuiEventHandler("CloseChatMessage", new Action<IDictionary<string, object>, CallbackDelegate>(OnNuiCloseChat));

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

        static void OnNuiCloseChat(IDictionary<string, object> data, CallbackDelegate cb)
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

        static void StoreChatPosition(IDictionary<string, object> data, CallbackDelegate cb)
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
                if (encodedMessage == "SERVER") return;

                if (string.IsNullOrEmpty(encodedMessage)) return;

                API.SendNuiMessage(encodedMessage);
            }
            catch (Exception ex)
            {
                if (!Player.PlayerInformation.IsDeveloper()) return;

                Debug.WriteLine($"{ex.Message}");
                Debug.WriteLine($"{encodedMessage}");
            }
        }

        static void HandleChatResult(IDictionary<string, object> data, CallbackDelegate cb)
        {
            try
            {
                EnableChatbox(false);
                API.SetNuiFocus(false, false);

                IDictionary<string, object> chatResult = data;

                if (!chatResult.ContainsKey("0") || string.IsNullOrWhiteSpace((string)chatResult["0"]))
                    return;

                string message = (String)chatResult["0"];
                string chatChannel = (String)chatResult["1"];

                if (Player.PlayerInformation.IsDeveloper())
                {
                    Log.Verbose($"{chatChannel} - {message}");
                }

                var spaceSplit = message.Split(' ');

                if (message.Substring(0, 1) == "/" && message.Length >= 2)
                {
                    API.ExecuteCommand(message.Substring(1));
                }
                else
                {
                    // message = DecodeEncodedNonAsciiCharacters(message);

                    Client.TriggerServerEvent("curiosity:Server:Chat:Message", message, chatChannel);
                }

                cb(new { ok = true });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HandleChatResult ERROR: ${ex.Message}");
            }
        }

        private static string DecodeEncodedNonAsciiCharacters(string value)
        {
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m =>
                {
                    return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
                });
        }

        static async Task OnChatTask()
        {
            try
            {
                SetTextChatEnabled(false);

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
