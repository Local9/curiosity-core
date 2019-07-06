using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;

namespace Curiosity.Client.net.Classes.Menus.PlayerCreator
{
    class PlayerSave
    {
        public static async void Init()
        {
            while (!PlayerCreatorMenu.MenuSetup)
                await BaseScript.Delay(0);

            PlayerCreatorMenu.menu.AddMenuItem(new MenuItem("Save Changes") { ItemData = "SAVE" });
        }

        public static void SaveCharacter()
        {
            string dataToSave = JsonConvert.SerializeObject(PlayerCreatorMenu.PlayerCharacter());
            Client.TriggerServerEvent("curiosity:Server:Character:Save", dataToSave);
        }
    }
}
