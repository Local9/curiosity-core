using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.Shared.Client.net.GameData;
using Curiosity.Shared.Client.net.Enums.Patrol;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net;

namespace Curiosity.Police.Client.net.Classes.Menus
{
    class MenuLoadout
    {
        private const string LOADOUT_PRIMARY_KEY = "Loadout:Primary";
        private const string LOADOUT_SECONDARY_KEY = "Loadout:Secondary";

        static Client client = Client.GetInstance();
        static Menu LoadoutMenu;

        static List<string> listPrimaryNames = new List<string>();
        static List<string> listSecondaryNames = new List<string>();

        static List<string> listPrimaryWeapons = new List<string>();
        static List<string> listSecondaryWeapons = new List<string>();

        static Dictionary<string, WeaponHash> weapons = new Dictionary<string, WeaponHash>();

        static MenuListItem menuListItemPrimary;
        static MenuListItem menuListItemSecondary;

        static MenuItem menuItemEquipExtras;

        static long GameTimeNow = Game.GameTime;
        static long GameTimeResupplied;

        static public void Init()
        {
            listPrimaryWeapons.Add("weapon_assaultsmg");
            listPrimaryWeapons.Add("weapon_assaultshotgun");
            listPrimaryWeapons.Add("weapon_carbinerifle");

            foreach(string str in listPrimaryWeapons)
            {
                listPrimaryNames.Add(ValidWeapons.weaponNames[str]);
            }

            listSecondaryWeapons.Add("weapon_combatpistol");
            listSecondaryWeapons.Add("weapon_pistol");
            listSecondaryWeapons.Add("weapon_snspistol");

            foreach (string str in listSecondaryWeapons)
            {
                listSecondaryNames.Add(ValidWeapons.weaponNames[str]);
            }
        }

        static public async Task OnWeaponTick()
        {
            if (Game.PlayerPed.IsInVehicle() && Game.PlayerPed.CurrentVehicle.ClassType == VehicleClass.Emergency && Environment.Job.DutyManager.IsOnDuty)
            {
                if (IsDisabledControlJustPressed(0, (int)Control.VehicleHorn))
                {
                    if ((GameTimeNow - GameTimeResupplied) > 120000)
                    {
                        GameTimeResupplied = Game.GameTime;

                        if (!string.IsNullOrEmpty(GetResourceKvpString(LOADOUT_PRIMARY_KEY)))
                        {
                            string primary = GetResourceKvpString(LOADOUT_PRIMARY_KEY);
                            if (primary == "weapon_assaultshotgun")
                            {
                                SetPedAmmo(Game.PlayerPed.Handle, (uint)GetHashKey(primary), 40);
                            }
                            else
                            {
                                SetPedAmmo(Game.PlayerPed.Handle, (uint)GetHashKey(primary), 120);
                            }
                        }

                        if (!string.IsNullOrEmpty(GetResourceKvpString(LOADOUT_SECONDARY_KEY)))
                        {
                            SetPedAmmo(Game.PlayerPed.Handle, (uint)GetHashKey(GetResourceKvpString(LOADOUT_SECONDARY_KEY)), 30);
                        }
                    }
                    else
                    {
                        Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "PD Vehicle", $"Sorry....", "You resupplied within the last 2 minutes, please reserve your ammunition more.", 2);
                    }
                }
            }
            await Task.FromResult(0);
        }

        static public void OpenMenu()
        {
            Game.PlayerPed.CanRagdoll = false;
            MenuBaseFunctions.MenuOpen();

            if (LoadoutMenu == null)
            {
                LoadoutMenu = new Menu("Loadout", "Select your weapons");
                LoadoutMenu.OnMenuOpen += LoadoutMenu_OnMenuOpen;
            }

            MenuController.AddMenu(LoadoutMenu);
            MenuController.EnableMenuToggleKeyOnController = false;

            LoadoutMenu.OpenMenu();

            LoadoutMenu.OnListItemSelect += LoadoutMenu_OnListItemSelect;
            LoadoutMenu.OnItemSelect += LoadoutMenu_OnItemSelect;
            LoadoutMenu.OnMenuClose += LoadoutMenu_OnMenuClose;
        }

        private static void LoadoutMenu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem == menuItemEquipExtras)
            {
                Game.PlayerPed.Weapons.Give(WeaponHash.Nightstick, 1, false, false);
                Game.PlayerPed.Weapons.Give(WeaponHash.StunGun, 1, false, false);
                Game.PlayerPed.Weapons.Give(WeaponHash.Flashlight, 1, false, false);

                if (Game.PlayerPed.Weapons.HasWeapon(WeaponHash.CombatPistol)) {
                    GiveWeaponComponentToPed(Game.PlayerPed.Handle, (uint)WeaponHash.CombatPistol, (uint)GetHashKey("COMPONENT_AT_PI_FLSH"));
                }
            }
        }

        private static void LoadoutMenu_OnMenuClose(Menu menu)
        {
            Game.PlayerPed.CanRagdoll = true;
            MenuBaseFunctions.MenuClose();
        }

        private static void LoadoutMenu_OnListItemSelect(Menu menu, MenuListItem listItem, int selectedIndex, int itemIndex)
        {
            if (listItem == menuListItemPrimary)
            {
                if (Game.PlayerPed.Weapons.HasWeapon((WeaponHash)GetHashKey(listPrimaryWeapons[selectedIndex]))) return;

                foreach (string str in listPrimaryWeapons)
                {
                    Game.PlayerPed.Weapons.Remove((WeaponHash)GetHashKey(str));
                }

                Game.PlayerPed.Weapons.Give((WeaponHash)GetHashKey(listPrimaryWeapons[selectedIndex]), 0, false, true);

                if (listPrimaryWeapons[selectedIndex] == "weapon_assaultshotgun")
                {
                    SetPedAmmo(Game.PlayerPed.Handle, (uint)GetHashKey(listPrimaryWeapons[selectedIndex]), 40);
                }
                else
                {
                    SetPedAmmo(Game.PlayerPed.Handle, (uint)GetHashKey(listPrimaryWeapons[selectedIndex]), 120);
                }

                SetResourceKvp(LOADOUT_PRIMARY_KEY, listPrimaryWeapons[selectedIndex]);
            }

            if (listItem == menuListItemSecondary)
            {
                if (Game.PlayerPed.Weapons.HasWeapon((WeaponHash)GetHashKey(listSecondaryWeapons[selectedIndex]))) return;

                foreach (string str in listSecondaryWeapons)
                {
                    Game.PlayerPed.Weapons.Remove((WeaponHash)GetHashKey(str));
                }

                Game.PlayerPed.Weapons.Give((WeaponHash)GetHashKey(listSecondaryWeapons[selectedIndex]), 0, false, true);
                SetPedAmmo(Game.PlayerPed.Handle, (uint)GetHashKey(listSecondaryWeapons[selectedIndex]), 30);

                SetResourceKvp(LOADOUT_SECONDARY_KEY, listSecondaryWeapons[selectedIndex]);
            }
        }

        private static void LoadoutMenu_OnMenuOpen(Menu menu)
        {
            LoadoutMenu.ClearMenuItems();

            Game.PlayerPed.Weapons.RemoveAll();

            menuListItemPrimary = new MenuListItem("Primary", listPrimaryNames, 0) { Description = "Press enter to select item." };
            menuListItemSecondary = new MenuListItem("Secondary", listSecondaryNames, 0) { Description = "Press enter to select item." };

            menuItemEquipExtras = new MenuItem("Add Tools");

            LoadoutMenu.AddMenuItem(menuListItemPrimary);
            LoadoutMenu.AddMenuItem(menuListItemSecondary);
            LoadoutMenu.AddMenuItem(menuItemEquipExtras);
        }
    }
}
