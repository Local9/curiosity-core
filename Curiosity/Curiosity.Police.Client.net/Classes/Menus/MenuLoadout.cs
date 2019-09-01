using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.Shared.Client.net.GameData;

namespace Curiosity.Police.Client.net.Classes.Menus
{
    class MenuLoadout
    {
        static Client client = Client.GetInstance();
        static Menu LoadoutMenu;

        static List<string> listPrimary = new List<string>();
        static List<string> listSecondary = new List<string>();

        static Dictionary<string, WeaponHash> weapons = new Dictionary<string, WeaponHash>();

        static public void Init()
        {
            listPrimary.Add(ValidWeapons.weaponNames["weapon_assaultsmg"]);
            listPrimary.Add(ValidWeapons.weaponNames["weapon_assaultshotgun"]);
            listPrimary.Add(ValidWeapons.weaponNames["weapon_assaultrifle"]);

            listSecondary.Add(ValidWeapons.weaponNames["weapon_combatpistol"]);
            listSecondary.Add(ValidWeapons.weaponNames["weapon_pistol"]);
            listSecondary.Add(ValidWeapons.weaponNames["weapon_snspistol"]);
        }

        static public void OpenMenu()
        {
            if (LoadoutMenu == null)
            {
                LoadoutMenu = new Menu("Loadout", "Select your weapons");
                LoadoutMenu.OnMenuOpen += LoadoutMenu_OnMenuOpen;
            }

            MenuController.AddMenu(LoadoutMenu);
            MenuController.EnableMenuToggleKeyOnController = false;

            LoadoutMenu.OpenMenu();

            LoadoutMenu.OnListItemSelect += LoadoutMenu_OnListItemSelect;
        }

        private static void LoadoutMenu_OnListItemSelect(Menu menu, MenuListItem listItem, int selectedIndex, int itemIndex)
        {
            Debug.WriteLine($"{listItem}");            
        }

        private static void LoadoutMenu_OnMenuOpen(Menu menu)
        {
            LoadoutMenu.ClearMenuItems();

            MenuListItem menuListItemPrimary = new MenuListItem("Primary", listPrimary, 0);
            MenuListItem menuListItemSecondary = new MenuListItem("Secondary", listSecondary, 0);

            LoadoutMenu.AddMenuItem(menuListItemPrimary);
            LoadoutMenu.AddMenuItem(menuListItemSecondary);
        }
    }
}
