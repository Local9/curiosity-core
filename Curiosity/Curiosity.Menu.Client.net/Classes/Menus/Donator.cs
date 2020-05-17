using CitizenFX.Core;
using Curiosity.Menus.Client.net.Classes.Player;
using Curiosity.Menus.Client.net.Classes.Scripts;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Menus.Client.net.Classes.Menus
{
    class Donator
    {
        static Client client = Client.GetInstance();

        static Menu donatorMenu;
        static MenuItem subMenuItem;
        static MenuItem menuItemDonatorVehicles = new MenuItem("Vehicles") { LeftIcon = MenuItem.Icon.INV_CAR };
        static MenuItem menuItemRemoveCompanion = new MenuItem("Remove Companion") { };

        static Dictionary<string, Tuple<PedHash, bool>> companions = new Dictionary<string, Tuple<PedHash, bool>>()
        {
            { "Husky", new Tuple<PedHash, bool>(PedHash.Husky, false) },
            { "Poodle", new Tuple<PedHash, bool>(PedHash.Poodle, false) },
            { "Pug", new Tuple<PedHash, bool>(PedHash.Pug, false) },
            { "Retriever", new Tuple<PedHash, bool>(PedHash.Retriever, false) },
            { "Rottweiler", new Tuple<PedHash, bool>(PedHash.Rottweiler, false) },
            { "Shepherd", new Tuple<PedHash, bool>(PedHash.Shepherd, false) },
            { "Cat", new Tuple<PedHash, bool>(PedHash.Cat, false) },
            { "Chimp", new Tuple<PedHash, bool>(PedHash.Chimp, true) },
            { "Westy", new Tuple<PedHash, bool>(PedHash.Westy, false) },
            { "Cop: Female", new Tuple<PedHash, bool>(PedHash.Cop01SFY, true) },
            { "Cop: Male", new Tuple<PedHash, bool>(PedHash.Cop01SMY, true) },
            { "Sheriff: Female", new Tuple<PedHash, bool>(PedHash.Sheriff01SFY, true) },
            { "Sheriff: Male", new Tuple<PedHash, bool>(PedHash.Sheriff01SMY, true) },
        };

        static int selectedCompanion = 0;

        static MenuListItem menuListItemCompanion = new MenuListItem("Companion", companions.Select(x => x.Key).ToList(), selectedCompanion);

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
            bool isItemEnabled = (Player.PlayerInformation.IsStaff() || Player.PlayerInformation.IsDonator());
            string description = isItemEnabled ? "Donator Options" : "~b~Pateron: ~y~https://www.patreon.com/lifev\n\n~w~If you have recently donated, please reconnect or contact support.";
            if (donatorMenu == null)
            {
                donatorMenu = new Menu("Donator Menu", "Thank you for your support!");

                donatorMenu.OnMenuOpen += Menu_OnMenuOpen;
                donatorMenu.OnMenuClose += Menu_OnMenuClose;

                donatorMenu.OnItemSelect += Menu_OnItemSelect;
                donatorMenu.OnListItemSelect += DonatorMenu_OnListItemSelect;
                subMenuItem = MenuBase.AddSubMenu(donatorMenu, "→→→", isItemEnabled, description, MenuItem.Icon.STAR);
            }

            subMenuItem.Enabled = isItemEnabled;

            MenuController.MainMenu.GetMenuItems().ForEach(mitem =>
            {
                if (mitem.Text == donatorMenu.MenuTitle)
                {
                    mitem.Enabled = isItemEnabled;
                    mitem.Description = description;
                    mitem.Label = "→→→";
                    mitem.RightIcon = MenuItem.Icon.NONE;
                    mitem.LeftIcon = MenuItem.Icon.STAR;
                };
            });
        }

        private static void DonatorMenu_OnListItemSelect(Menu menu, MenuListItem listItem, int selectedIndex, int itemIndex)
        {
            if (listItem == menuListItemCompanion)
            {
                KeyValuePair<string, Tuple<PedHash, bool>> selection = companions.ElementAt(selectedIndex);
                Companion.CreateCompanion(selection.Value.Item1, selection.Value.Item2);
            }
        }

        private static void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem == menuItemDonatorVehicles)
            {
                Client.TriggerEvent("curiosity:Client:Vehicle:OpenDonatorVehicles");
                menu.CloseMenu();
            }

            if (menuItem == menuItemRemoveCompanion)
            {
                Companion.RemoveCompanion();
            }
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            MenuBase.MenuOpen(false);
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            MenuBase.MenuOpen(true);

            bool isItemEnabled = (Player.PlayerInformation.IsStaff() || Player.PlayerInformation.IsDonator());
            subMenuItem.Enabled = isItemEnabled;

            menu.ClearMenuItems();
            menu.AddMenuItem(menuItemDonatorVehicles);
            menu.AddMenuItem(menuListItemCompanion);

            if (PlayerInformation.IsDeveloper() || PlayerInformation.IsProjectManager())
            {
                if (!companions.ContainsKey("DEV: Cow"))
                {
                    companions.Add("DEV: Cow", new Tuple<PedHash, bool>(PedHash.Cow, false));
                    companions.Add("DEV: Mountain Lion", new Tuple<PedHash, bool>(PedHash.MountainLion, false));
                    companions.Add("DEV: Chiken Hawk", new Tuple<PedHash, bool>(PedHash.ChickenHawk, false));
                    companions.Add("DEV: Gull", new Tuple<PedHash, bool>(PedHash.Seagull, false));
                    companions.Add("DEV: Boar", new Tuple<PedHash, bool>(PedHash.Boar, false));
                    companions.Add("DEV: Coyote", new Tuple<PedHash, bool>(PedHash.Coyote, false));
                    companions.Add("DEV: Hen", new Tuple<PedHash, bool>(PedHash.Hen, false));
                    companions.Add("DEV: Deer", new Tuple<PedHash, bool>(PedHash.Deer, false));
                    companions.Add("DEV: Pig", new Tuple<PedHash, bool>(PedHash.Pig, false));
                    companions.Add("DEV: Rabbit", new Tuple<PedHash, bool>(PedHash.Rabbit, false));
                    companions.Add("DEV: Rat", new Tuple<PedHash, bool>(PedHash.Rat, false));
                    companions.Add("DEV: Pidgen", new Tuple<PedHash, bool>(PedHash.Pigeon, false));
                    companions.Add("DEV: Franklin", new Tuple<PedHash, bool>(PedHash.Franklin, true));
                    companions.Add("DEV: Trevor", new Tuple<PedHash, bool>(PedHash.Trevor, true));
                    companions.Add("DEV: Michael", new Tuple<PedHash, bool>(PedHash.Michael, true));
                }
            }

            menuListItemCompanion.ListItems = companions.Select(x => x.Key).ToList();

            menu.AddMenuItem(menuItemRemoveCompanion);
        }
    }
}
