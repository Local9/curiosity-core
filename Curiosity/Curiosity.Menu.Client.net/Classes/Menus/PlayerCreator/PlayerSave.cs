using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;

namespace Curiosity.Menus.Client.net.Classes.Menus.PlayerCreator
{
    class PlayerSave
    {
        static bool saveBuffer = false;

        public static async void Init()
        {
            while (!PlayerCreatorMenu.MenuSetup)
                await BaseScript.Delay(0);

            PlayerCreatorMenu.menu.AddMenuItem(new MenuItem("Save Changes") { ItemData = "SAVE" });
        }

        public static async void SaveCharacter()
        {
            if (saveBuffer)
                return;

            saveBuffer = true;

            string dataToSave = JsonConvert.SerializeObject(PlayerCreatorMenu.PlayerCharacter());

            await BaseScript.Delay(0);

            Client.TriggerServerEvent("curiosity:Server:Character:Save", dataToSave);

            await BaseScript.Delay(2000);

            saveBuffer = false;
        }
    }
}
