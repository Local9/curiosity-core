using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment.UI.Mobile.Api.Entity
{
    class Application
    {
        public int Id;
        public string Name;
        public AppIcons Icon;

        public int GetID { get { return Id; } }
        public string GetName { get { return Name; } }
        public AppIcons GetIcon { get { return Icon; } }

        static Dictionary<int, Screen> Screens = new Dictionary<int, Screen>();

        public static void CreateListScreen(string header)
        {

        }
    }
}
