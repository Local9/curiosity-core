using Curiosity.Global.Shared.net.Enums.Mobile;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Mobile.Client.net.Mobile.Api
{
    class Application
    {
        public static int NUM_APPS = 0;
        public static Dictionary<int, Application> applications = new Dictionary<int, Application>();

        protected int Id;
        protected int Position;
        protected string Name;
        protected AppIcons Icon;
        protected List<Screen> screens = new List<Screen>();
        protected Screen screen;
        protected Task task;

        public int GetID { get { return Id; } }
        public int GetPosition { get { return Position; } }
        public string GetName { get { return Name; } }
        public AppIcons GetIcon { get { return Icon; } }
        public Task GetTask { get { return task; } }
        public List<Screen> Screens { get { return screens; } }
        public Screen LauncherScreen { get { return screen; } set { screen = value; } }
        public Func<bool> StartTask;
        public Func<bool> StopTask;

        public Application(string name, AppIcons appIcon, int position)
        {
            Name = name;
            Icon = appIcon;
            Position = position;
            applications.Add(position, this);
            NUM_APPS++;
        }

        public void AddScreen(Screen screen)
        {
            screens.Add(screen);
            if (LauncherScreen == null) // Just as a safeguard.
            {
                LauncherScreen = screen;
            }
        }

        public Screen AddScreenType(string header, View view)
        {
            Screen listScreen = new Screen(this, header, view);
            AddScreen(listScreen);
            return listScreen;
        }

        public Screen AddListScreen(string header)
        {
            Screen listScreen = new Screen(this, header, Screen.LIST);
            AddScreen(listScreen);
            return listScreen;
        }
    }
}
