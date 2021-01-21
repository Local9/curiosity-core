using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Curiosity.Interface.Client.Managers
{
    public class ChatManager : Manager<ChatManager>
    {
        static bool IsChatboxOpen = false;

        const string COMMAND_OPEN_CHAT = "open_chat";
        const string COMMAND_CLOSE_CHAT = "close_chat";

        public override void Begin()
        {
            EventSystem.Attach("chat:receive", new EventCallback(metadata =>
            {
                API.SendNuiMessage(metadata.Find<string>(0));
                return null;
            }));

            Instance.AttachNuiHandler("SendChatMessage", new EventCallback(metadata =>
            {
                EnableChatbox(false);
                API.SetNuiFocus(false, false);

                string message = metadata.Find<string>(0);
                string chatChannel = metadata.Find<string>(1);

                if (string.IsNullOrWhiteSpace(message))
                    return null;

                var spaceSplit = message.Split(' ');

                if (message.Substring(0, 1) == "/" && message.Length >= 2)
                {
                    API.ExecuteCommand(message.Substring(1));
                }
                else
                {
                    EventSystem.Send("chat:global", message, chatChannel);
                }
                return null;
            }));

            Instance.AttachNuiHandler("CloseChatMessage", new EventCallback(metadata =>
            {
                EnableChatbox(false);
                return null;
            }));

            API.RegisterKeyMapping(COMMAND_OPEN_CHAT, "Open Chat", "keyboard", "T");
            API.RegisterKeyMapping(COMMAND_CLOSE_CHAT, "Close Chat", "keyboard", "ESC");
            API.RegisterCommand(COMMAND_OPEN_CHAT, new Action(OnOpenChatCommand), false);
            API.RegisterCommand(COMMAND_CLOSE_CHAT, new Action(OnCloseChatCommand), false);
        }

        private void OnCloseChatCommand()
        {
            if (IsChatboxOpen)
            {
                EnableChatbox(false);
                API.SetPedCanSwitchWeapon(Game.PlayerPed.Handle, true);
            }
        }

        private void OnOpenChatCommand()
        {
            if (!IsChatboxOpen)
            {
                EnableChatbox(true);
                API.SetPedCanSwitchWeapon(Game.PlayerPed.Handle, false);
            }
        }

        static void EnableChatbox(bool state)
        {
            try
            {
                if (IsChatboxOpen != state)
                {
                    ChatState chatState = new ChatState();
                    chatState.showChat = state;
                    API.SendNuiMessage(JsonConvert.SerializeObject(chatState));

                    IsChatboxOpen = state;

                    if (IsChatboxOpen)
                    {
                        API.SetNuiFocus(true, true);
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
                EnableChatbox(false);
            }
        }

        static void OnCloseChat()
        {
            API.EnableControlAction(0, (int)Control.CursorScrollUp, true);
            API.EnableControlAction(0, (int)Control.CursorScrollDown, true);
            API.SetPedCanSwitchWeapon(Game.PlayerPed.Handle, true);
            API.SetNuiFocus(false, false);
        }
    }
}
