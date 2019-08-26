using MenuAPI;

namespace Curiosity.Menus.Client.net.Classes.Menus.MissionCreator
{
    class MissionMenu
    {
        static Client client = Client.GetInstance();
        static Menu menu = new Menu("Mission Maker", "WORK IN PROGRESS");

        public static void Init()
        {
            if (Player.PlayerInformation.IsDeveloper())
            {

                MenuBase.AddSubMenu(menu);
            }
        }
    }
}
