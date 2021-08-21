using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Interface.Menus.VehicleMods.SubMenu;
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

        // TO DO
        /*
         * MOVE:
         * Neon into submenu class file
         * Create color submenu class file
         * 
         * */

        private UIMenu mainMenu;
        private UIMenu vehicleModMenu;
        private VehicleModSubMenu vehicleModSubMenu = new VehicleModSubMenu();
        private UIMenu vehicleNeonMenu;
        private VehicleNeonSubMenu vehicleNeonSubMenu = new VehicleNeonSubMenu();
        private UIMenu vehicleColorMenu;
        private VehicleColorSubMenu vehicleColorSubMenu = new VehicleColorSubMenu();

        private UIMenuCheckboxItem uiChkXenonHeadlights;
        private UIMenuCheckboxItem uiChkTurbo;
        private UIMenuCheckboxItem uiChkBulletProofTires;
        private UIMenuCheckboxItem uiChkCustomWheels;
        private UIMenuCheckboxItem uiChkTireSmoke;

        private UIMenuListItem uiLstWindowTint;
        private UIMenuListItem uiLstHeadlightColor;
        private UIMenuItem closeMenu;

        int oldWheelIndex = 0;

        List<dynamic> headlightColor = new List<dynamic>() { "White", "Blue", "Electric Blue", "Mint Green", "Lime Green", "Yellow", "Golden Shower", "Orange", "Red", "Pony Pink", "Hot Pink", "Purple", "Blacklight", "Default Xenon" };
        List<dynamic> windowTints = new List<dynamic>() { "Stock", "None", "Limo", "Light Smoke", "Dark Smoke", "Pure Black", "Green" };

        public override void Begin()
        {

            _MenuPool = new MenuPool();

            // create menu
            mainMenu = new UIMenu("Vehicle Mod Menu", "Modify your vehicle");

            _MenuPool.Add(mainMenu);

            vehicleModMenu = _MenuPool.AddSubMenu(mainMenu, "Mods");
            vehicleModSubMenu.Create(vehicleModMenu);

            vehicleNeonMenu = _MenuPool.AddSubMenu(mainMenu, "Neon Kits");
            vehicleNeonSubMenu.Create(vehicleNeonMenu);

            vehicleColorMenu = _MenuPool.AddSubMenu(mainMenu, "Colors");
            vehicleColorSubMenu.Create(vehicleColorMenu);

            uiLstWindowTint = new UIMenuListItem("Window Tint", windowTints, 0);
            uiLstWindowTint.Description = "Apply tint to your windows.";
            mainMenu.AddItem(uiLstWindowTint);

            uiChkCustomWheels = new UIMenuCheckboxItem("Custom Wheels", false);
            uiChkCustomWheels.Description = "Add or remove ~y~custom~s~ wheels.";
            mainMenu.AddItem(uiChkCustomWheels);

            uiChkBulletProofTires = new UIMenuCheckboxItem("Bullet Proof Tires", false);
            mainMenu.AddItem(uiChkBulletProofTires);

            uiChkTurbo = new UIMenuCheckboxItem("Turbo", false);
            mainMenu.AddItem(uiChkTurbo);

            uiChkXenonHeadlights = new UIMenuCheckboxItem("Xenon Headlights", false);
            mainMenu.AddItem(uiChkXenonHeadlights);

            uiLstHeadlightColor = new UIMenuListItem("Headlight Color", headlightColor, 0);
            mainMenu.AddItem(uiLstHeadlightColor);

            closeMenu = new UIMenuItem("Close");
            mainMenu.AddItem(closeMenu);

            mainMenu.OnMenuStateChanged += MainMenu_OnMenuStateChanged;
            mainMenu.OnItemSelect += MainMenu_OnItemSelect;
            mainMenu.OnListChange += MainMenu_OnListChange;
            mainMenu.OnCheckboxChange += MainMenu_OnCheckboxChange;
        }

        private void MainMenu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked)
        {
            Vehicle vehicle = Game.PlayerPed.CurrentVehicle;

            if (checkboxItem == uiChkBulletProofTires)
            {
                SetVehicleTyresCanBurst(vehicle.Handle, !Checked);
            }
            else if (checkboxItem == uiChkTurbo)
            {
                ToggleVehicleMod(vehicle.Handle, 18, Checked);
            }
            else if (checkboxItem == uiChkXenonHeadlights)
            {
                ToggleVehicleMod(vehicle.Handle, 22, Checked);
            }
            else if (checkboxItem == uiChkCustomWheels)
            {
                SetVehicleMod(vehicle.Handle, 23, GetVehicleMod(vehicle.Handle, 23), !GetVehicleModVariation(vehicle.Handle, 23));

                // If the player is on a motorcycle, also change the back wheels.
                if (IsThisModelABike((uint)GetEntityModel(vehicle.Handle)))
                {
                    SetVehicleMod(vehicle.Handle, 24, GetVehicleMod(vehicle.Handle, 24), GetVehicleModVariation(vehicle.Handle, 23));
                }
            }
        }

        private void MainMenu_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            Vehicle vehicle = Game.PlayerPed.CurrentVehicle;

            if (listItem == uiLstWindowTint)
            {
                switch (newIndex)
                {
                    case 1:
                        SetVehicleWindowTint(vehicle.Handle, 0); // None
                        break;
                    case 2:
                        SetVehicleWindowTint(vehicle.Handle, 5); // Limo
                        break;
                    case 3:
                        SetVehicleWindowTint(vehicle.Handle, 3); // Light Smoke
                        break;
                    case 4:
                        SetVehicleWindowTint(vehicle.Handle, 2); // Dark Smoke
                        break;
                    case 5:
                        SetVehicleWindowTint(vehicle.Handle, 1); // Pure Black
                        break;
                    case 6:
                        SetVehicleWindowTint(vehicle.Handle, 6); // Green
                        break;
                    case 0:
                    default:
                        SetVehicleWindowTint(vehicle.Handle, 4); // Stock
                        break;
                }
            }
            else if (listItem == uiLstHeadlightColor)
            {
                if (newIndex == 13) // default
                {
                    _SetHeadlightsColorOnVehicle(vehicle, 255);
                }
                else if (newIndex > -1 && newIndex < 13)
                {
                    _SetHeadlightsColorOnVehicle(vehicle, newIndex);
                }
            }
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


            //uiChkBulletProofTires.Checked = !GetVehicleTyresCanBurst(vehicle.Handle);
            //uiChkCustomWheels.Checked = IsToggleModOn(vehicle.Handle, 23);
            //uiChkTurbo.Checked = IsToggleModOn(vehicle.Handle, 18);
            //uiChkXenonHeadlights.Checked = IsToggleModOn(vehicle.Handle, 22);
        }

        internal static int _GetHeadlightsColorFromVehicle(Vehicle vehicle)
        {
            if (vehicle != null && vehicle.Exists())
            {
                if (IsToggleModOn(vehicle.Handle, 22))
                {
                    int val = GetVehicleHeadlightsColour(vehicle.Handle);
                    if (val > -1 && val < 13)
                    {
                        return val;
                    }
                    return -1;
                }
            }
            return -1;
        }

        internal static void _SetHeadlightsColorOnVehicle(Vehicle veh, int newIndex)
        {

            if (veh != null && veh.Exists() && veh.Driver == Game.PlayerPed)
            {
                if (newIndex > -1 && newIndex < 13)
                {
                    SetVehicleHeadlightsColour(veh.Handle, newIndex);
                }
                else
                {
                    SetVehicleHeadlightsColour(veh.Handle, -1);
                }
            }
        }
    }
}
