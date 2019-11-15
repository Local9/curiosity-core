using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using MenuAPI;

namespace Curiosity.Missions.Client.net.Scripts.Police.MenuHandler
{
    class SuspectMenu
    {
        static Client client = Client.GetInstance();

        static Ped suspect;

        static Menu menu;

        // Buttons
        static MenuItem miRunName = new MenuItem("Request ID & Run name");
        static MenuItem miRelease = new MenuItem("Release");
        // Core
        static MenuItem miLeaveVehicle = new MenuItem("Leave Vehicle");
        static MenuItem miFollow = new MenuItem("Follow") { Enabled = false };
        // Options
        static MenuItem miBreathalyzer = new MenuItem("Breathalyzer");
        static MenuItem miDrugTest = new MenuItem("Drug Test");
        static MenuItem miSearch = new MenuItem("Search");
        // Arrest
        static MenuItem miArrest = new MenuItem("Arrest") { Enabled = false };
        static MenuItem miEnterVehicle = new MenuItem("Put in car") { Enabled = false, Description = "Put ped in the back of the police car" };

        static public void Open(Ped ped)
        {
            if (ped == null)
            {
                throw new ArgumentNullException();
            }

            suspect = ped;

            MenuController.DisableBackButton = true;
            MenuController.DontOpenAnyMenu = false;

            client.RegisterTickHandler(OnTask);

            if (menu == null)
            {
                menu = new Menu("Suspect Menu", "Please use the options below");

                menu.OnMenuClose += Menu_OnMenuClose;
            }
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            suspect.Task.ClearAll();
            suspect.MarkAsNoLongerNeeded();

            MenuController.DontOpenAnyMenu = true;
        }

        static async Task OnTask()
        {
            try
            {
                if (suspect == null)
                {
                    client.DeregisterTickHandler(OnTask);
                }
            }
            catch (Exception ex)
            {
                client.DeregisterTickHandler(OnTask);
            }
            await BaseScript.Delay(0);
        }
    }
}
