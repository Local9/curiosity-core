using CitizenFX.Core.Native;
using Newtonsoft.Json;
using System;

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
            client.RegisterEventHandler("curiosity:Client:Interface:ShowForum", new Action(OnShowForum));
            client.RegisterNuiEventHandler("closeForum", new Action(OnCloseForum));
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
