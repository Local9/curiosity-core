using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Shared.Client.net;
using static Curiosity.Shared.Client.net.Helper.NativeWrappers;
using static Curiosity.Shared.Client.net.Helper.ControlHelper;

namespace Curiosity.Chat.Client.net
{
    public class Chat : BaseScript
    {
        static bool isChatInputActive = false;
        static bool isChatScrollEnabled = true;
        static bool isChatInputActivating = false;
        static bool previousState = false;

        const string chatMessageColor = "#e7bd42";
        const float localChatAoe = 25f;

        string role = "|";


        public Chat()
        {
            SetNuiFocus(false);
            SetTextChatEnabled(false);

            RegisterEventHandler("curiosity:Client:Player:Role", new Action<string>(UpdatePlayerRole));
            RegisterEventHandler("curiosity:Client:Chat:Message", new Action<string, string, string>(ChatMessage));
            RegisterEventHandler("curiosity:Client:Chat:EnableScroll", new Action<bool>(EnableChatScroll));
            RegisterEventHandler("curiosity:Client:Chat:EnableChatBox", new Action<bool>(EnableChatBox));

            RegisterEventHandler("curiosity:Client:Chat:Side", new Action<string>(OnChatSide));

            RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));

            RegisterNuiCallbackType("chatResult");
            RegisterEventHandler("__cfx_nui:chatResult", new Action<System.Dynamic.ExpandoObject>(HandleChatResult));

            Tick += UpdateChat;
        }

        void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            string chatPosition = API.GetResourceKvpString("curiosity:chat:position");

            if (string.IsNullOrEmpty(chatPosition))
                chatPosition = "chat-right";

            OnChatSide(chatPosition);
        }

        void OnChatSide(string side)
        {
            API.SetResourceKvp("curiosity:chat:position", side);
            SendNuiMessage($@"{{""class"": ""{side}""}}");
        }

        void UpdatePlayerRole(string roleIn)
        {
            switch(roleIn)
            {
                case "Developer":
                    role = "DEV";
                    break;
                case "Project Manager":
                    role = "PM";
                    break;
                case "Community Manager":
                    role = "CM";
                    break;
                case "Head Admin":
                    role = "HA";
                    break;
                case "Moderator":
                case "Administrator":
                case "Senior Admin":
                    role = "";
                    break;
                case "Helper":
                    break;
                case "Donator":
                    role = "*";
                    break;
                default:
                    role = "|";
                    break;
            }
        }

        /// <summary>
        /// Send a chat message to all users on the server
        /// </summary>
        internal void ChatMessage(string name, string color, string message)
        {
            try
            {
                SendNuiMessage($@"{{""name"": ""{name}"", ""color"": ""{color}"", ""message"": ""{message.Replace(@"'", @"&apos;").Replace(@"""", @"&quot;")}""}}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ChatMessage ERROR: ${ex.Message}");
            }
        }

        internal void TriggerChatAction(string name)
        {
            SendNuiMessage($@"{{""meta"": ""{name}""}}");
        }

        internal void EnableChatScroll(bool enable)
        {
            isChatScrollEnabled = enable;
        }

        internal void EnableChatBox(bool enable)
        {
            string triggerEvent = "enableChatBox";
            if (!enable) triggerEvent = "disableChatBox";
            TriggerChatAction(triggerEvent);
        }

        internal void HandleChatResult(System.Dynamic.ExpandoObject data)
        {
            try
            {
                // The below might be handled by the Controls class when implemented
                TriggerChatAction("scrollBottom");

                EnableControlAction(0, Control.CursorScrollUp, true);
                EnableControlAction(0, Control.CursorScrollDown, true);
                SetPedCanSwitchWeapon(Game.PlayerPed, true);
                SetNuiFocus(false);
                isChatInputActive = false;

                IDictionary<string, object> chatResult = data;

                if (!chatResult.ContainsKey("message") || string.IsNullOrWhiteSpace((string)chatResult["message"]))
                    return;

                string message = (String)chatResult["message"];

                var spaceSplit = message.Split(' ');

                // Debug.WriteLine(message);

                if (message.Substring(0, 1) == "/" && message.Length >= 2)
                {
                    API.ExecuteCommand(message.Substring(1));
                }
                else
                {
                    // async void ChatMessage([FromSource]Player player, string message, string color, string scope)
                    TriggerServerEvent("curiosity:Server:Chat:Message", role, message, chatMessageColor, "all");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HandleChatResult ERROR: ${ex.Message}");
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task UpdateChat()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            try
            {
                // Currently it seems this assembly loads way earlier than the Lua ever did
                // So SetTextChatEnabled has to be called at a later point than before
                // Not sure what would be a suitable solution, so temporarily
                // it's done on each tick... Overkill.
                SetTextChatEnabled(false);

                if (isChatScrollEnabled)
                {
                    if (Game.IsControlJustPressed(0, Control.ReplayCameraUp)) // PgUp
                    {
                        TriggerChatAction("scrollUp");
                    }
                    else if (Game.IsControlJustPressed(0, Control.ReplayCameraDown)) // PgDn
                    {
                        TriggerChatAction("scrollDown");
                    }
                }

                if (isChatInputActive && Game.IsDisabledControlPressed(0, Control.CursorScrollUp)) // Scrollwheel Up
                {
                    TriggerChatAction("scrollUp");
                }
                else if (isChatInputActive && Game.IsDisabledControlPressed(0, Control.CursorScrollDown)) // Scrollwheel Down
                {
                    TriggerChatAction("scrollDown");
                }

                if (Game.IsControlJustPressed(0, Control.FrontendCancel)) // Escape
                {
                    isChatInputActive = false;
                    isChatInputActivating = false;
                    EnableControlAction(0, Control.CursorScrollUp, true); // Scrollwheel Up
                    EnableControlAction(0, Control.CursorScrollDown, true); // Scrollwheel Down
                    SetPedCanSwitchWeapon(Game.PlayerPed, true); // Disable weapon select
                    SetNuiFocus(false);

                    TriggerChatAction("forceCloseChatBox");
                }

                if (!isChatInputActive && Game.IsControlPressed(0, Control.MpTextChatAll))
                {
                    isChatInputActive = true;
                    isChatInputActivating = true;
                    TriggerChatAction("openChatBox");
                }

                if (isChatInputActivating && !Game.IsControlPressed(0, Control.MpTextChatAll))
                {
                    SetNuiFocus(true);
                    isChatInputActivating = false;
                }

                if (isChatInputActive)
                {
                    TriggerChatAction("focusChatBox");
                    DisableControlAction(0, Control.CursorScrollUp, true);
                    DisableControlAction(0, Control.CursorScrollDown, true);
                    SetPedCanSwitchWeapon(Game.PlayerPed, false);
                }

                if (previousState != isChatInputActive)
                {
                    TriggerEvent("curiosity:Client:Chat:ChatboxActive", isChatInputActive);
                    previousState = isChatInputActive;
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"UpdateChat ERROR: ${ex.Message}");
            }
        }

        void RegisterEventHandler(string name, Delegate action)
        {
            EventHandlers[name] += action;
        }
    }
}
