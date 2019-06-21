using MenuAPI;

namespace Curiosity.Client.net.Classes.Menus
{
    class Inventory
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            MenuItem menuItem = new MenuItem("Inventory") { ItemData = "Inventory", Description = "Open Character Inventory" };
            PlayerMenu.AddMenuItem(menuItem);
        }
    }
}
