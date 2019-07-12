using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment.UI.Mobile.Api
{
    enum AppIcons
    {
        Camera = 1,
        Chat = 2,
        Empty = 3,
        Messaging = 4,
        Contacts = 5,
        Internet = 6,
        ContactsPlus = 11,
        Tasks = 12,
        Group = 14,
        Settings = 24,
        Warning = 27,
        Games = 35,
        RightArrow = 38,
        Tasks2 = 39,
        Target = 40,
        Trackify = 42,
        Cloud = 43,
        Broadcast = 49,
        VLSI = 54,
        Bennys = 56,
        SecuroServ = 57,
        Coords = 58,
        RSS = 59
    }

    class Applications
    {
        public static Dictionary<int, Entity.Application> applications = new Dictionary<int, Entity.Application>();

        public static void CreateApp(string name, AppIcons icon)
        {
            int appId = applications.Count + 1;
            applications[appId] = new Entity.Application { Id = appId, Name = name, Icon = icon };
        }

    }
}
