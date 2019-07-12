using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment.UI.Mobile.Api
{
    class Screens
    {
        public static Dictionary<int, Entity.Screen> ScreenDictionary = new Dictionary<int, Entity.Screen>();

        void CreateBaseScreen(Entity.Application app, string header, int screenType)
        {
            int id = ScreenDictionary.Count + 1;
            ScreenDictionary[id] = new Entity.Screen(id, header, screenType);

        }
    }
}
