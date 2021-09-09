using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers
{
    public class ChatManager : Manager<ChatManager>
    {
        static bool IsChatboxOpen = false;

        const string COMMAND_OPEN_CHAT = "open_chat";
        const string COMMAND_CLOSE_CHAT = "close_chat";

        public override void Begin()
        {
            EventSystem.Attach("chat:receive", new AsyncEventCallback(async metadata =>
            {
                await Session.Loading();

                JsonBuilder jsonBuilder = new JsonBuilder();
                jsonBuilder.Add("operation", "CHAT");
                jsonBuilder.Add("subOperation", "NEW_MESSAGE");
                jsonBuilder.Add("message", new {
                    role = metadata.Find<string>(1),
                    channel = metadata.Find<string>(3),
                    activeJob = metadata.Find<string>(4),
                    timestamp = DateTime.Now.ToString("HH:mm"),
                    name = metadata.Find<string>(0),
                    message = metadata.Find<string>(2),
                    showChat = !API.IsPauseMenuActive(),
                    avatar = metadata.Find<string>(5),
                });

                string nuiMessage = jsonBuilder.Build();


                API.SendNuiMessage(nuiMessage);

                return null;
            }));

            Instance.AttachNuiHandler("SendChatMessage", new EventCallback(metadata =>
            {
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
                    EventSystem.Send("chat:message", message, chatChannel);
                }
                return null;
            }));

            Instance.AttachNuiHandler("CloseChatMessage", new EventCallback(metadata =>
            {
                EnableChatbox(false);
                return null;
            }));

            Instance.AttachNuiHandler("ReleaseMouse", new EventCallback(metadata =>
            {
                OnCloseChat();
                IsChatboxOpen = false;
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
                if (API.IsPauseMenuActive()) return;

                if (IsChatboxOpen != state)
                {
                    JsonBuilder jsonBuilder = new JsonBuilder();
                    jsonBuilder.Add("operation", "CHAT");
                    jsonBuilder.Add("subOperation", state ? "SHOW_CHAT" : "HIDE_CHAT");
                    jsonBuilder.Add("focus", state);

                    API.SendNuiMessage(jsonBuilder.Build());

                    IsChatboxOpen = state;

                    if (IsChatboxOpen)
                    {
                        API.SetNuiFocus(true, IsChatboxOpen);
                        API.SetNuiFocusKeepInput(!IsChatboxOpen);
                    }
                    else
                    {
                        API.SetNuiFocus(false, false);
                        API.SetNuiFocusKeepInput(false);
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
            API.SetNuiFocusKeepInput(false);
        }

        [TickHandler]
        private async Task OnCheckPauseMenu()
        {
            if (API.IsPauseMenuActive() && IsChatboxOpen)
            {
                API.SetNuiFocus(false, false);
                API.SetNuiFocusKeepInput(false);
                OnCloseChat();
            }
            await BaseScript.Delay(500);
        }
    }
}
