using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers
{
    public class ChatManager : Manager<ChatManager>
    {
        static bool PreviousChatboxState = false;

        public override void Begin()
        {
            Instance.EventRegistry["chat:receive"] += new Action<string>(OnChatReceived);

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
                    // EventSystem.Send("chat:global", message, chatChannel);
                    BaseScript.TriggerServerEvent("chat:global", message, chatChannel);
                }
                return null;
            }));

            Instance.AttachNuiHandler("CloseChatMessage", new EventCallback(metadata =>
            {
                EnableChatbox(false);
                return null;
            }));
        }

        private void OnChatReceived(string message)
        {
            API.SendNuiMessage(message);
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

        [TickHandler(SessionWait = true)]
        private async Task OnTick()
        {
            try
            {
                if (Game.IsControlPressed(0, Control.FrontendCancel))
                {
                    EnableChatbox(false);
                }
                else if (Game.IsControlJustPressed(0, Control.MpTextChatAll) || Game.IsControlJustReleased(0, Control.MpTextChatAll))
                {
                    EnableChatbox(true);
                }

                if (PreviousChatboxState)
                {
                    API.DisableControlAction(0, (int)Control.CursorScrollUp, true);
                    API.DisableControlAction(0, (int)Control.CursorScrollDown, true);
                    API.SetPedCanSwitchWeapon(Game.PlayerPed.Handle, false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnChatTask ERROR: ${ex.Message}");
                EnableChatbox(false);
            }
        }
    }
}
