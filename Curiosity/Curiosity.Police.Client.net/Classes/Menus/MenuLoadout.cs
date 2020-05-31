using CitizenFX.Core;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.GameData;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Police.Client.net.Classes.Menus
{
    class MenuLoadout
    {
        private const string LOADOUT_PRIMARY_KEY = "Loadout:Primary";
        private const string LOADOUT_SECONDARY_KEY = "Loadout:Secondary";

        static Client client = Client.GetInstance();
        public static Menu mainLoadoutMenu;

        static List<string> listPrimaryNames = new List<string>();
        static List<string> listSecondaryNames = new List<string>();

        static List<string> listPrimaryWeapons = new List<string>();
        static List<string> listSecondaryWeapons = new List<string>();

        static Dictionary<string, WeaponHash> weapons = new Dictionary<string, WeaponHash>();
        static Dictionary<MenuItem, string> weaponComponents = new Dictionary<MenuItem, string>();

        static MenuListItem menuListItemPrimary;
        static MenuListItem menuListItemSecondary;

        static MenuItem menuItemEquipExtras;
        static MenuItem menuItemEquipArmor;

        static Vector3 positionMenuOpen;

        static long GameTimeResupplied;
        static long gameTimePressed = Game.GameTime;

        static public void Init()
        {
            client.RegisterEventHandler("curiosity:Player:Loadout:Resupply", new Action(OnWeaponResupply));

            listPrimaryWeapons.Add("weapon_smg");
            listPrimaryWeapons.Add("weapon_pumpshotgun");
            listPrimaryWeapons.Add("weapon_carbinerifle");
            listPrimaryWeapons.Add("weapon_sniperrifle");

            foreach (string str in listPrimaryWeapons)
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

        static public async void OnWeaponResupply()
        {
            if ((Game.GameTime - GameTimeResupplied) > 120000)
            {
                GameTimeResupplied = Game.GameTime;

                int primaryCost = 0;
                int secondaryCost = 0;

                if (!string.IsNullOrEmpty(GetResourceKvpString(LOADOUT_PRIMARY_KEY)))
                {
                    string primary = GetResourceKvpString(LOADOUT_PRIMARY_KEY);
                    if (primary == "weapon_pumpshotgun")
                    {
                        int currentAmmo = GetAmmoInPedWeapon(Game.PlayerPed.Handle, (uint)GetHashKey(primary));
                        primaryCost = 40 - currentAmmo;
                        SetPedAmmo(Game.PlayerPed.Handle, (uint)GetHashKey(primary), 40);
                    }
                    else
                    {
                        int currentAmmo = GetAmmoInPedWeapon(Game.PlayerPed.Handle, (uint)GetHashKey(primary));
                        primaryCost = 120 - currentAmmo;
                        SetPedAmmo(Game.PlayerPed.Handle, (uint)GetHashKey(primary), 120);
                    }
                }

                if (!string.IsNullOrEmpty(GetResourceKvpString(LOADOUT_SECONDARY_KEY)))
                {
                    uint secondary = (uint)GetHashKey(GetResourceKvpString(LOADOUT_SECONDARY_KEY));
                    int currentAmmo = GetAmmoInPedWeapon(Game.PlayerPed.Handle, secondary);
                    secondaryCost = 50 - currentAmmo;
                    SetPedAmmo(Game.PlayerPed.Handle, secondary, 50);
                }

                Game.PlayerPed.Armor = 100;

                int amtToPay = (primaryCost + secondaryCost);

                if (amtToPay < 100)
                {
                    amtToPay = 100;
                }

                int charge = amtToPay + 20;

                Client.TriggerServerEvent("curiosity:Server:Bank:DecreaseCash", Player.PlayerInformation.playerInfo.Wallet, charge);

                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "PD Vehicle", $"Ammunition Resupplied", $"~b~Charged: ~g~$~w~{charge}~s~~n~Please wait 2 minutes to resupply again.", 2);

                await Client.Delay(500);
            }
            else
            {
                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "PD Vehicle", $"Sorry....", "You resupplied within the last 2 minutes, please reserve your ammunition more.", 2);
            }

        }

        static async Task CheckDistance()
        {
            if (positionMenuOpen.DistanceToSquared(Game.PlayerPed.Position) > 5f)
            {
                if (mainLoadoutMenu != null)
                    mainLoadoutMenu.CloseMenu();

                client.DeregisterTickHandler(CheckDistance);
            }
            await Task.FromResult(0);
        }

        static public void OpenMenu()
        {
            if (Environment.Job.DutyManager.IsPoliceJobActive) return;

            MenuController.DontOpenAnyMenu = false;
            Game.PlayerPed.CanRagdoll = false;
            MenuBaseFunctions.MenuOpen();

            MenuController.EnableMenuToggleKeyOnController = false;

            positionMenuOpen = Game.PlayerPed.Position;

            client.RegisterTickHandler(CheckDistance);

            if (mainLoadoutMenu == null)
            {
                mainLoadoutMenu = new Menu("Loadout", "Select your weapons");
                mainLoadoutMenu.OnMenuOpen += LoadoutMenu_OnMenuOpen;

                mainLoadoutMenu.OnListItemSelect += LoadoutMenu_OnListItemSelect;
                mainLoadoutMenu.OnListIndexChange += LoadoutMenu_OnListIndexChange;
                mainLoadoutMenu.OnItemSelect += LoadoutMenu_OnItemSelect;
                mainLoadoutMenu.OnMenuClose += LoadoutMenu_OnMenuClose;

                MenuController.AddMenu(mainLoadoutMenu);
                MenuController.EnableMenuToggleKeyOnController = false;
            }

            mainLoadoutMenu.ClearMenuItems();
            mainLoadoutMenu.OpenMenu();
        }

        private static void LoadoutMenu_OnListIndexChange(Menu menu, MenuListItem listItem, int oldSelectionIndex, int newSelectionIndex, int itemIndex)
        {
            if (listItem == menuListItemPrimary)
            {
                if (Game.PlayerPed.Weapons.HasWeapon((WeaponHash)GetHashKey(listPrimaryWeapons[newSelectionIndex]))) return;

                string weaponHash = listPrimaryWeapons[newSelectionIndex];

                foreach (string str in listPrimaryWeapons)
                {
                    Game.PlayerPed.Weapons.Remove((WeaponHash)GetHashKey(str));
                }

                Game.PlayerPed.Weapons.Give((WeaponHash)GetHashKey(weaponHash), 0, false, true);

                if (weaponHash == "weapon_assaultshotgun")
                {
                    SetPedAmmo(Game.PlayerPed.Handle, (uint)GetHashKey(listPrimaryWeapons[newSelectionIndex]), 40);
                    GiveWeaponComponentToPed(Game.PlayerPed.Handle, (uint)WeaponHash.AssaultShotgun, (uint)GetHashKey("COMPONENT_AT_PI_FLSH"));
                }
                else
                {
                    if (weaponHash == "weapon_carbinerifle")
                    {
                        if (Player.PlayerInformation.playerInfo.Skills.ContainsKey("policexp"))
                        {
                            int xp = Player.PlayerInformation.playerInfo.Skills["policexp"].Value;

                            if (xp > 500)
                            {
                                GiveWeaponComponentToPed(Game.PlayerPed.Handle, (uint)WeaponHash.CarbineRifle, (uint)GetHashKey("COMPONENT_AT_AR_FLSH")); // correct
                            }

                            if (xp >= 5000)
                            {
                                GiveWeaponComponentToPed(Game.PlayerPed.Handle, (uint)WeaponHash.CarbineRifle, (uint)GetHashKey("COMPONENT_AT_AR_AFGRIP")); // correct
                                SetPedAmmo(Game.PlayerPed.Handle, (uint)GetHashKey(listPrimaryWeapons[newSelectionIndex]), 180);
                            }
                            else
                            {
                                SetPedAmmo(Game.PlayerPed.Handle, (uint)GetHashKey(listPrimaryWeapons[newSelectionIndex]), 120);
                            }

                            if (xp >= 10000)
                            {
                                GiveWeaponComponentToPed(Game.PlayerPed.Handle, (uint)WeaponHash.CarbineRifle, (uint)GetHashKey("COMPONENT_CARBINERIFLE_CLIP_02")); // correct
                            }
                            
                            if (xp >= 100000)
                            {
                                GiveWeaponComponentToPed(Game.PlayerPed.Handle, (uint)WeaponHash.CarbineRifle, (uint)WeaponComponentHash.CarbineRifleVarmodLuxe); // correct
                            }
                        }
                    }
                    else
                    {
                        SetPedAmmo(Game.PlayerPed.Handle, (uint)GetHashKey(listPrimaryWeapons[newSelectionIndex]), 120);
                    }
                }

                SetResourceKvp(LOADOUT_PRIMARY_KEY, listPrimaryWeapons[newSelectionIndex]);
            }

            if (listItem == menuListItemSecondary)
            {
                if (Game.PlayerPed.Weapons.HasWeapon((WeaponHash)GetHashKey(listSecondaryWeapons[newSelectionIndex]))) return;

                foreach (string str in listSecondaryWeapons)
                {
                    Game.PlayerPed.Weapons.Remove((WeaponHash)GetHashKey(str));
                }

                Game.PlayerPed.Weapons.Give((WeaponHash)GetHashKey(listSecondaryWeapons[newSelectionIndex]), 0, false, true);
                SetPedAmmo(Game.PlayerPed.Handle, (uint)GetHashKey(listSecondaryWeapons[newSelectionIndex]), 50);

                SetResourceKvp(LOADOUT_SECONDARY_KEY, listSecondaryWeapons[newSelectionIndex]);
            }
        }

        private static void LoadoutMenu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem == menuItemEquipExtras)
            {
                Game.PlayerPed.Weapons.Give(WeaponHash.Nightstick, 1, false, false);
                Game.PlayerPed.Weapons.Give(WeaponHash.StunGun, 1, false, false);
                Game.PlayerPed.Weapons.Give(WeaponHash.Flashlight, 1, false, false);

                if (Game.PlayerPed.Weapons.HasWeapon(WeaponHash.CombatPistol))
                {
                    GiveWeaponComponentToPed(Game.PlayerPed.Handle, (uint)WeaponHash.CombatPistol, (uint)GetHashKey("COMPONENT_AT_PI_FLSH"));
                }
            }

            if (menuItem == menuItemEquipArmor)
            {
                Game.PlayerPed.Armor = 100;
            }
        }

        private static void LoadoutMenu_OnMenuClose(Menu menu)
        {
            MenuController.DontOpenAnyMenu = true;
            Game.PlayerPed.CanRagdoll = true;
            MenuBaseFunctions.MenuClose();
            mainLoadoutMenu.ClearMenuItems();
            mainLoadoutMenu = null;
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
                SetPedAmmo(Game.PlayerPed.Handle, (uint)GetHashKey(listSecondaryWeapons[selectedIndex]), 50);

                SetResourceKvp(LOADOUT_SECONDARY_KEY, listSecondaryWeapons[selectedIndex]);
            }
        }

        private static void LoadoutMenu_OnMenuOpen(Menu menu)
        {
            mainLoadoutMenu.ClearMenuItems(); ;

            Game.PlayerPed.Weapons.RemoveAll();

            menuListItemPrimary = new MenuListItem("Primary", listPrimaryNames, 0);
            menuListItemSecondary = new MenuListItem("Secondary", listSecondaryNames, 0);

            menuItemEquipExtras = new MenuItem("Collect Tools") { Description = "Press ~r~ENTER~s~ to equip tools." };
            menuItemEquipArmor = new MenuItem("Equip Armor") { Description = "Press ~r~ENTER~s~ to equip armor." };

            mainLoadoutMenu.AddMenuItem(menuListItemPrimary);
            mainLoadoutMenu.AddMenuItem(menuListItemSecondary);
            mainLoadoutMenu.AddMenuItem(menuItemEquipExtras);
            mainLoadoutMenu.AddMenuItem(menuItemEquipArmor);

        }
    }
}
