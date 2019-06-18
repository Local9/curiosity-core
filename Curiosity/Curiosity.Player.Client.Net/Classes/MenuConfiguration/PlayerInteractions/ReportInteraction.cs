using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using MenuAPI;
using System.Drawing;
using Curiosity.Shared.Client.net.Helper;
using GlobalEntities = Curiosity.Global.Shared.net.Entity;
using GlobalEnums = Curiosity.Global.Shared.net.Enums;

namespace Curiosity.Client.net.Classes.MenuConfiguration.PlayerInteractions
{
    class ReportInteraction
    {
        static Client client = Client.GetInstance();
        static List<GlobalEntities.LogType> reportReasons = new List<GlobalEntities.LogType>();
        static int playerHandle = 0;

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Menu:Report", new Action<string>(SetupReportReasons));
            client.RegisterTickHandler(OnMenuOptions);
        }

        static async Task OnMenuOptions()
        {
            while (reportReasons.Count == 0)
            {
                await Client.Delay(2000);
                Client.TriggerServerEvent("curiosity:Server:Menu:Reasons", (int)GlobalEnums.LogGroup.Report);
                if (reportReasons.Count > 0)
                {
                    client.DeregisterTickHandler(OnMenuOptions);
                }
            }
            await Task.FromResult(0);
        }

        static void SetupReportReasons(string json)
        {
            reportReasons = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GlobalEntities.LogType>>(json);
        }

        public static Menu CreateMenu(string menuTitle, CitizenFX.Core.Player player)
        {
            Menu menu = new Menu(menuTitle, $"Report: {player.Name}");

            menu.OnMenuOpen += (_menu) => {
                foreach(GlobalEntities.LogType logType in reportReasons)
                {
                    menu.AddMenuItem(new MenuItem(logType.Description) { ItemData = logType });
                }
            };

            menu.OnMenuClose += (_menu) =>
            {
                _menu.ClearMenuItems();
            };

            menu.OnItemSelect += (_menu, _item, _index) =>
            {
                GlobalEntities.LogType lt = _item.ItemData;
                Client.TriggerServerEvent("curiosity:Server:Player:Report", player.ServerId, $"{lt.LogTypeId}|{lt.Description}");
                _menu.CloseMenu();
            };

            return menu;
        }
    }
}
