using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Menus.Client.net.Extensions;
using MenuAPI;
using System;
using System.Linq;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Menus.Client.net.Classes.Menus
{
    class Developer
    {
        static bool menuSetup = false;

        static bool _debugAreas = false, _developerNpcUiEnable = false, __developerVehicleUiEnable = false, _godMode = false, _allWeapons = false;

        static Client client = Client.GetInstance();
        static Menu menu = new Menu("Developer Menu", "Various Settings or options");

        // Vehicle Options
        static MenuItem mItemRepair = new MenuItem("Repair Vehicle");
        static MenuItem mItemRefuel = new MenuItem("Refuel Vehicle");
        // Debuging
        static MenuCheckboxItem menuCheckboxItemDebugAreas = new MenuCheckboxItem("Draw Area Debug", _debugAreas);
        static MenuCheckboxItem menuCheckboxGodMode = new MenuCheckboxItem("God Mode", _godMode);
        static MenuCheckboxItem menuCheckboxGiveAllWeapons = new MenuCheckboxItem("All Weapons", _allWeapons);
        static MenuCheckboxItem menuCheckboxItemDeveloperNpcUi = new MenuCheckboxItem("Show Interactive NPC UI", _developerNpcUiEnable);
        static MenuCheckboxItem menuCheckboxItemDeveloperVehUi = new MenuCheckboxItem("Show Interactive Veh UI", __developerVehicleUiEnable);

        public static void Init()
        {
            client.RegisterEventHandler("playerSpawned", new Action<dynamic>(OnPlayerSpawned));
            client.RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));
            SetupMenu();
        }

        static void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;
            SetupMenu();
        }

        static void OnPlayerSpawned(dynamic dynData)
        {
            SetupMenu();
        }

        static void SetupMenu()
        {
            if (!Player.PlayerInformation.IsDeveloper()) return;

            if (menuSetup) return;

            menu.OnMenuOpen += Menu_OnMenuOpen;
            menu.OnMenuClose += Menu_OnMenuClose;

            menu.OnItemSelect += Menu_OnItemSelect;
            menu.OnCheckboxChange += Menu_OnCheckboxChange;

            menuSetup = true;

            MenuBase.AddSubMenu(menu);
        }

        private static void Menu_OnCheckboxChange(Menu menu, MenuCheckboxItem menuItem, int itemIndex, bool newCheckedState)
        {
            if (menuItem == menuCheckboxGodMode)
            {
                string msg = "~b~God Mode~s~: ~r~Disabled";

                if (newCheckedState)
                    msg = "~b~God Mode~s~: ~g~Enabled";

                Screen.ShowNotification(msg);

                Game.PlayerPed.IsInvincible = newCheckedState;
                Game.PlayerPed.ClearBloodDamage();
                Game.PlayerPed.ClearLastWeaponDamage();
            }

            if (menuItem == menuCheckboxGiveAllWeapons)
            {
                string msg = "~b~All Weapons~s~: ~r~Disabled";

                if (newCheckedState)
                    msg = "~b~All Weapons~s~: ~g~Enabled";

                Screen.ShowNotification(msg);

                if (newCheckedState)
                {
                    Enum.GetValues(typeof(WeaponHash)).Cast<WeaponHash>().ToList().ForEach(w =>
                    {
                        Game.PlayerPed.Weapons.Give(w, 999, false, true);
                        Game.PlayerPed.Weapons[w].InfiniteAmmo = true;
                        Game.PlayerPed.Weapons[w].InfiniteAmmoClip = true;
                    });
                }
                else
                {
                    Game.PlayerPed.Weapons.RemoveAll();
                }
            }

            if (menuItem == menuCheckboxItemDebugAreas)
            {
                _debugAreas = newCheckedState;
                Client.TriggerEvent("curiosity:Client:Player:Environment:DrawAreas", _debugAreas);
            }

            if (menuItem == menuCheckboxItemDeveloperNpcUi)
            {
                _developerNpcUiEnable = newCheckedState;

                Decorators.Set(Game.PlayerPed.Handle, "player::npc::debug", _developerNpcUiEnable);
            }

            if (menuItem == menuCheckboxItemDeveloperVehUi)
            {
                __developerVehicleUiEnable = newCheckedState;

                Decorators.Set(Game.PlayerPed.Handle, "player::veh::debug", __developerVehicleUiEnable);
            }
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            MenuBase.MenuOpen(false);
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            MenuBase.MenuOpen(true);
            menu.ClearMenuItems();

            menu.AddMenuItem(menuCheckboxGodMode);
            menu.AddMenuItem(menuCheckboxGiveAllWeapons);

            bool enableVehicleOptions = false;

            if (Client.CurrentVehicle != null)
            {
                enableVehicleOptions = Client.CurrentVehicle.Exists();
            }

            mItemRefuel.Enabled = enableVehicleOptions;
            menu.AddMenuItem(mItemRefuel);
            mItemRepair.Enabled = enableVehicleOptions;
            menu.AddMenuItem(mItemRepair);

            menu.AddMenuItem(menuCheckboxItemDebugAreas);
            menu.AddMenuItem(menuCheckboxItemDeveloperNpcUi);
            menu.AddMenuItem(menuCheckboxItemDeveloperVehUi);
        }

        private static void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem == mItemRefuel)
                BaseScript.TriggerEvent("curiosity:Client:Vehicle:DevRefuel");

            if (menuItem == mItemRepair)
                Client.TriggerEvent("curiosity:Client:Vehicle:DevRepair");
        }
    }
}
