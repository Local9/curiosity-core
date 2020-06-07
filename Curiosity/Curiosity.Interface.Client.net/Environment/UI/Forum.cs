using CitizenFX.Core.Native;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Curiosity.Interface.Client.net.Environment.UI
{
    class ForumMessage
    {
        public bool showForum;
    }
    class Forum
    {
        static Client client = Client.GetInstance();
        static public void Init()
        {
            API.RegisterCommand("closeGuides", new Action<int, List<object>, string>(OnCloseForumCommand), false);

            client.RegisterEventHandler("curiosity:Client:Interface:ShowForum", new Action(OnShowForum));
            client.RegisterNuiEventHandler("closeForum", new Action(OnCloseForum));
        }

        private static void OnCloseForumCommand(int arg1, List<object> arg2, string arg3)
        {
            OnCloseForum();
        }

        static void OnShowForum()
        {
            API.SetNuiFocus(true, true);
            API.SendNuiMessage(JsonConvert.SerializeObject(new ForumMessage { showForum = true }));
        }
        static void OnCloseForum()
        {
            API.SetNuiFocus(false, false);
            API.SendNuiMessage(JsonConvert.SerializeObject(new ForumMessage { showForum = false }));
        }
    }
}
