using CitizenFX.Core;
using Curiosity.Global.Shared.net;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Shared.Client.net;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Police.Client.net.Classes.Menus
{
    class LoadoutMenu
    {
        static Menu menu;
        static Client client = Client.GetInstance();
        static Random random = new Random();

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Police:Loadout", new Action<string>(OnUpdateMenu));
        }

        public static void OpenMenu(int spawnId)
        {
            Client.TriggerServerEvent("curiosity:Server:Police:Loadouts");

            if (menu == null)
            {
                menu = new Menu("Loadout", "Select a Loadout");
                menu.OnMenuOpen += Menu_OnMenuOpen;
                menu.OnMenuClose += Menu_OnMenuClose;
                menu.OnItemSelect += Menu_OnItemSelect;

                MenuController.AddMenu(menu);
                MenuController.EnableMenuToggleKeyOnController = false;
                MenuController.EnableManualGCs = false;
            }

            menu.ClearMenuItems();
            menu.OpenMenu();
        }

        public static void CloseMenu()
        {
            if (menu != null)
                menu.CloseMenu();
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            menu.ClearMenuItems();
            MenuBaseFunctions.MenuClose();
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            MenuBaseFunctions.MenuOpen();
            menu.AddMenuItem(new MenuItem("Loading...") { Enabled = false });
        }

        private static void OnUpdateMenu(string encodedJson)
        {

            string skillDesc = string.Empty;
            try
            {
                menu.ClearMenuItems();

                string json = Encode.BytesToStringConverted(System.Convert.FromBase64String(encodedJson));
                List<VehicleItem> vehicleItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<VehicleItem>>(json);

                if (Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.DEVELOPER)
                {
                    VehicleItem dev = vehicleItems[0];
                    dev.VehicleHashString = "tezeract";

                    menu.AddMenuItem(new MenuItem("Developer Car") { ItemData = dev });
                }

                foreach (VehicleItem vehicle in vehicleItems)
                {
                    MenuItem item = new MenuItem(vehicle.Name) { ItemData = vehicle };

                    if (vehicle.UnlockRequirementValue == 0)
                    {
                        item.Enabled = true;
                    }
                    else
                    {
                        if (!Player.PlayerInformation.playerInfo.Skills.ContainsKey(vehicle.UnlockRequiredSkill))
                        {
                            item.Enabled = false;
                        }
                        else
                        {
                            item.Enabled = (Player.PlayerInformation.playerInfo.Skills[vehicle.UnlockRequiredSkill].Value >= vehicle.UnlockRequirementValue);
                        }
                    }

                    item.Description = $"Requires: {vehicle.UnlockRequiredSkillDescription} >= {vehicle.UnlockRequirementValue}";
                    skillDesc = vehicle.UnlockRequiredSkillDescription;
                    menu.AddMenuItem(item);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error getting list, possible that you have no experience in the required skill. Skill Required: {skillDesc}");
            }
        }

        private static void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            
        }
    }
}
