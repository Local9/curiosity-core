using MenuAPI;
using Newtonsoft.Json;

namespace Curiosity.Client.net.Classes.Menus.PlayerCreator
{
    class PlayerSave
    {
        public static void Init()
        {
            PlayerCreatorMenu.menu.AddMenuItem(new MenuItem("Save Changes") { ItemData = "SAVE" });
        }

        public static void SaveCharacter()
        {
            string dataToSave = JsonConvert.SerializeObject(PlayerCreatorMenu.playerCharacter);
            Client.TriggerServerEvent("curiosity:Server:Character:Save", dataToSave);
        }
    }
}
