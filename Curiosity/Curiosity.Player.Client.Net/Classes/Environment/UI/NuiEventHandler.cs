using CitizenFX.Core;
using Curiosity.Shared.Client.net.Helper;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Client.net.Classes.Environment.UI
{
    class NuiEventHandler
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            client.RegisterNuiEventHandler("ClosePanel", new Action<dynamic>(ClosePanel));
        }

        static void ClosePanel(dynamic obj)
        {
            SetNuiFocus(false, false);
        }
    }
}
