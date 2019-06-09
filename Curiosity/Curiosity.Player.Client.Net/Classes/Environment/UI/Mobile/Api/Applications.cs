using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment.UI.Mobile.Api
{
    class Applications
    {
        public static Dictionary<int, Entity.Application> applications = new Dictionary<int, Entity.Application>();

        public static void CreateApp(string name, int icon)
        {
            int appId = applications.Count + 1;
            applications[appId] = new Entity.Application { Id = appId, Name = name, Icon = icon };
        }
    }
}
