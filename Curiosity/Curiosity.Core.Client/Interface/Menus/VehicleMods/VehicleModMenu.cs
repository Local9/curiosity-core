using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Managers;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Interface.Menus.VehicleMods
{
    public class VehicleModMenu : Manager<VehicleModMenu>
    {
        public static MenuPool _MenuPool;
        // Menus
        private UIMenu mainMenu;
        private UIMenu vehicleModMenu;
        private UIMenu vehicleNeonMenu;
        private UIMenu vehicleColorMenu;
        private UIMenuItem closeMenu;

        UIMenuCheckboxItem uIMenuCheckboxDriftTires;
        UIMenuCheckboxItem uIMenuCheckboxDriftTiresSuspension;
        bool driftTiresEnabled = false;
        bool driftTiresSuspensionEnabled = false;

        public override void Begin()
        {
            _MenuPool = new MenuPool();
            _MenuPool.MouseEdgeEnabled = false;
            // create menu
            mainMenu = new UIMenu("Vehicle Mod Menu", "Modify your vehicle");

            _MenuPool.Add(mainMenu);

            vehicleModMenu = _MenuPool.AddSubMenu(mainMenu, "Mods");
            vehicleNeonMenu = _MenuPool.AddSubMenu(mainMenu, "Neon Kits");
            vehicleColorMenu = _MenuPool.AddSubMenu(mainMenu, "Colors");

            uIMenuCheckboxDriftTires = new UIMenuCheckboxItem("Enable Drift Tires", driftTiresEnabled);
            uIMenuCheckboxDriftTiresSuspension = new UIMenuCheckboxItem("Drift Suspension", driftTiresSuspensionEnabled);

            closeMenu = new UIMenuItem("Close");
            mainMenu.AddItem(closeMenu);

            mainMenu.OnMenuStateChanged += MainMenu_OnMenuStateChanged;
            mainMenu.OnItemSelect += MainMenu_OnItemSelect;
        }

        private void MainMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == closeMenu)
            {
                Instance.DetachTickHandler(OnMenuCreate);

                mainMenu.InstructionalButtons.Clear();

                if (mainMenu.Visible)
                    mainMenu.Visible = false;
            }
        }

        public void OpenMenu()
        {
            Instance.AttachTickHandler(OnMenuCreate);
        }

        private async Task OnMenuCreate()
        {
            try
            {
                _MenuPool.ProcessMenus();
                _MenuPool.ProcessMouse();

                if (!_MenuPool.IsAnyMenuOpen() && mainMenu is not null) // KEEP IT FUCKING OPEN
                    mainMenu.Visible = true;
            }
            catch (KeyNotFoundException ex)
            {

            }
            catch (IndexOutOfRangeException ex)
            {

            }
            catch (Exception ex)
            {
                Logger.Debug(ex, $"OnMenuCreate");
            }
        } 

        private void MainMenu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (Equals(MenuState.Opened, state) || Equals(MenuState.ChangeForward, state))
            {
                Vehicle vehicle = Game.PlayerPed.CurrentVehicle;
                driftTiresEnabled = GetDriftTyresEnabled(vehicle.Handle);
                uIMenuCheckboxDriftTires.Checked = driftTiresEnabled;
                UpdateMods();
            }
        }

        void UpdateMods()
        {
            // clear the menu
            vehicleModMenu.Clear();

            vehicleModMenu.AddItem(uIMenuCheckboxDriftTires);
            vehicleModMenu.AddItem(uIMenuCheckboxDriftTiresSuspension);

            vehicleModMenu.OnCheckboxChange += (sender, item, check) =>
            {
                Vehicle vehicle = Game.PlayerPed.CurrentVehicle;

                if (item == uIMenuCheckboxDriftTires)
                {
                    SetDriftTyresEnabled(vehicle.Handle, check);
                }

                if (item == uIMenuCheckboxDriftTires)
                {
                    SetReduceDriftVehicleSuspension(vehicle.Handle, check);
                }
            };

            // get the vehicle they are sat in
            Vehicle vehicle = Game.PlayerPed.CurrentVehicle;
            // check it
            if (vehicle is not null && vehicle.Exists() && !vehicle.IsDead)
            {
                // Set the vehicle mod kit
                vehicle.Mods.InstallModKit();
                // Get the vehicles mods
                VehicleMod[] vehicleMods = vehicle.Mods.GetAllMods();
                // loop over them
                foreach(VehicleMod mod in vehicleMods)
                {
                    if (mod.ModCount > 0)
                    {
                        string name = mod.LocalizedModTypeName;
                        UIMenu menu = _MenuPool.AddSubMenu(vehicleModMenu, $"{name}");
                        for (var i = 0; i < mod.ModCount; i++)
                        {
                            UIMenuItem uIMenuItem = new UIMenuItem($"{name} #{i + 1}");
                            uIMenuItem.ItemData = new { mod = mod.ModType, variation = i };
                            menu.AddItem(uIMenuItem);
                        }

                        menu.OnItemSelect += (sender, item, index) => {
                            vehicle = Game.PlayerPed.CurrentVehicle;
                            vehicle.Mods.InstallModKit();

                            var vehMod = item.ItemData?.mod;
                            var variation = item.ItemData?.variation;

                            SetVehicleMod(vehicle.Handle, (int)vehMod, variation, false);
                            Logger.Debug($"Vehicle Mod: {vehMod}:{variation}");
                        };
                    }
                    else
                    {
                        Logger.Debug($"Vehicle Mod Ignored: {mod}");
                    }
                }
            }
        }

        static string ToProperString(string inputString)
        {
            var outputString = "";
            var prevUpper = true;
            foreach (char c in inputString)
            {
                if (char.IsLetter(c) && c != ' ' && c == char.Parse(c.ToString().ToUpper()))
                {
                    if (prevUpper)
                    {
                        outputString += $"{c}";
                    }
                    else
                    {
                        outputString += $" {c}";
                    }
                    prevUpper = true;
                }
                else
                {
                    prevUpper = false;
                    outputString += c.ToString();
                }
            }
            while (outputString.IndexOf("  ") != -1)
            {
                outputString = outputString.Replace("  ", " ");
            }
            return outputString;
        }
    }
}
