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

        // TO DO
        /*
         * MOVE:
         * Neon into submenu class file
         * Create color submenu class file
         * 
         * */

        private UIMenu mainMenu;
        private UIMenu vehicleModMenu;
        private UIMenu vehicleNeonMenu;
        private UIMenu vehicleColorMenu;
        private UIMenuCheckboxItem uiChkXenonHeadlights;
        private UIMenuCheckboxItem uiChkTurbo;
        private UIMenuCheckboxItem uiChkBulletProofTires;
        private UIMenuCheckboxItem uiChkCustomWheels;
        private UIMenuCheckboxItem uiChkTireSmoke;
        private UIMenuListItem uiLstWindowTint;
        private UIMenuListItem uiLstWheelType;
        private UIMenuListItem uiLstHeadlightColor;
        private UIMenuItem closeMenu;

        int oldWheelIndex = 0;

        List<dynamic> headlightColor = new List<dynamic>() { "White", "Blue", "Electric Blue", "Mint Green", "Lime Green", "Yellow", "Golden Shower", "Orange", "Red", "Pony Pink", "Hot Pink", "Purple", "Blacklight", "Default Xenon" };
        List<dynamic> windowTints = new List<dynamic>() { "Stock", "None", "Limo", "Light Smoke", "Dark Smoke", "Pure Black", "Green" };
        List<dynamic> wheelTypes = new List<dynamic>()
                {
                    "Sports",       // 0
                    "Muscle",       // 1
                    "Lowrider",     // 2
                    "SUV",          // 3
                    "Offroad",      // 4
                    "Tuner",        // 5
                    "Bike Wheels",  // 6
                    "High End",     // 7
                    "Benny's (1)",  // 8
                    "Benny's (2)",  // 9
                    "Open Wheel",   // 10
                    "Street"        // 11
                };

        private UIMenuCheckboxItem uiChkUnderglowFront = new UIMenuCheckboxItem("Enable Front Light", false);
        private UIMenuCheckboxItem uiChkUnderglowBack = new UIMenuCheckboxItem("Enable Back Light", false);
        private UIMenuCheckboxItem uiChkUnderglowLeft = new UIMenuCheckboxItem("Enable Left Light", false);
        private UIMenuCheckboxItem uiChkUnderglowRight = new UIMenuCheckboxItem("Enable Right Light", false);
        private UIMenuListItem uiLstUnderglowColor;

        public override void Begin()
        {
            var underglowColorsList = new List<dynamic>();
            for (int i = 0; i < 13; i++)
            {
                underglowColorsList.Add(GetLabelText($"CMOD_NEONCOL_{i}"));
            }

            _MenuPool = new MenuPool();
            _MenuPool.MouseEdgeEnabled = false;
            // create menu
            mainMenu = new UIMenu("Vehicle Mod Menu", "Modify your vehicle");

            _MenuPool.Add(mainMenu);

            vehicleModMenu = _MenuPool.AddSubMenu(mainMenu, "Mods");
            vehicleNeonMenu = _MenuPool.AddSubMenu(mainMenu, "Neon Kits");
            vehicleColorMenu = _MenuPool.AddSubMenu(mainMenu, "Colors");

            uiLstUnderglowColor = new UIMenuListItem(GetLabelText("CMOD_NEON_1"), underglowColorsList, 0);

            vehicleNeonMenu.AddItem(uiLstUnderglowColor);
            vehicleNeonMenu.AddItem(uiChkUnderglowFront);
            vehicleNeonMenu.AddItem(uiChkUnderglowBack);
            vehicleNeonMenu.AddItem(uiChkUnderglowLeft);
            vehicleNeonMenu.AddItem(uiChkUnderglowRight);

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

            vehicleNeonMenu.OnCheckboxChange += VehicleNeonMenu_OnCheckboxChange;
            vehicleNeonMenu.OnListChange += VehicleNeonMenu_OnListChange;
            vehicleNeonMenu.OnMenuStateChanged += VehicleNeonMenu_OnMenuStateChanged;
        }

        private void VehicleNeonMenu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.ChangeForward)
            {
                Vehicle vehicle = Game.PlayerPed.CurrentVehicle;
                if (vehicle.Mods.HasNeonLights)
                {
                    uiChkUnderglowBack.Checked = vehicle.Mods.HasNeonLight(VehicleNeonLight.Back) && vehicle.Mods.IsNeonLightsOn(VehicleNeonLight.Back);
                    uiChkUnderglowFront.Checked = vehicle.Mods.HasNeonLight(VehicleNeonLight.Front) && vehicle.Mods.IsNeonLightsOn(VehicleNeonLight.Front);
                    uiChkUnderglowLeft.Checked = vehicle.Mods.HasNeonLight(VehicleNeonLight.Left) && vehicle.Mods.IsNeonLightsOn(VehicleNeonLight.Left);
                    uiChkUnderglowRight.Checked = vehicle.Mods.HasNeonLight(VehicleNeonLight.Right) && vehicle.Mods.IsNeonLightsOn(VehicleNeonLight.Right);

                    uiChkUnderglowBack.Enabled = true;
                    uiChkUnderglowFront.Enabled = true;
                    uiChkUnderglowLeft.Enabled = true;
                    uiChkUnderglowRight.Enabled = true;

                    uiChkUnderglowRight.SetLeftBadge(UIMenuItem.BadgeStyle.None);
                    uiChkUnderglowLeft.SetLeftBadge(UIMenuItem.BadgeStyle.None);
                    uiChkUnderglowBack.SetLeftBadge(UIMenuItem.BadgeStyle.None);
                    uiChkUnderglowFront.SetLeftBadge(UIMenuItem.BadgeStyle.None);
                }
                else
                {
                    uiChkUnderglowBack.Enabled = false;
                    uiChkUnderglowFront.Enabled = false;
                    uiChkUnderglowLeft.Enabled = false;
                    uiChkUnderglowRight.Enabled = false;

                    uiChkUnderglowBack.Checked = false;
                    uiChkUnderglowFront.Checked = false;
                    uiChkUnderglowLeft.Checked = false;
                    uiChkUnderglowRight.Checked = false;

                    uiChkUnderglowRight.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                    uiChkUnderglowLeft.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                    uiChkUnderglowBack.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                    uiChkUnderglowFront.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                }
            }
        }

        private void VehicleNeonMenu_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (listItem == uiLstUnderglowColor)
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    Vehicle veh = Game.PlayerPed.CurrentVehicle;
                    if (veh.Mods.HasNeonLights)
                    {
                        veh.Mods.NeonLightsColor = GetColorFromIndex(newIndex);
                    }
                }
            }
        }

        private void VehicleNeonMenu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked)
        {
            if (Game.PlayerPed.IsInVehicle())
            {
                Vehicle veh = Game.PlayerPed.CurrentVehicle;
                if (veh.Mods.HasNeonLights)
                {
                    veh.Mods.NeonLightsColor = GetColorFromIndex(uiLstUnderglowColor.Index);
                    if (checkboxItem == uiChkUnderglowLeft)
                    {
                        veh.Mods.SetNeonLightsOn(VehicleNeonLight.Left, veh.Mods.HasNeonLight(VehicleNeonLight.Left) && Checked);
                    }
                    else if (checkboxItem == uiChkUnderglowRight)
                    {
                        veh.Mods.SetNeonLightsOn(VehicleNeonLight.Right, veh.Mods.HasNeonLight(VehicleNeonLight.Right) && Checked);
                    }
                    else if (checkboxItem == uiChkUnderglowBack)
                    {
                        veh.Mods.SetNeonLightsOn(VehicleNeonLight.Back, veh.Mods.HasNeonLight(VehicleNeonLight.Back) && Checked);
                    }
                    else if (checkboxItem == uiChkUnderglowFront)
                    {
                        veh.Mods.SetNeonLightsOn(VehicleNeonLight.Front, veh.Mods.HasNeonLight(VehicleNeonLight.Front) && Checked);
                    }
                }
            }
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
            if (Equals(MenuState.Opened, state) || Equals(MenuState.ChangeForward, state))
            {
                UpdateMods();
            }
        }

        void UpdateMods()
        {
            // clear the menu
            vehicleModMenu.Clear();
            uiLstWheelType = null;

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
                    if (mod.ModType == VehicleModType.FrontWheel && uiLstWheelType is null)
                    {
                        if (!vehicle.Model.IsBoat && !vehicle.Model.IsHelicopter && !vehicle.Model.IsPlane && !vehicle.Model.IsBicycle && !vehicle.Model.IsTrain)
                        {
                            uiLstWheelType = new UIMenuListItem("Wheel Type", wheelTypes, MathUtil.Clamp(GetVehicleWheelType(vehicle.Handle), 0, 11));
                            vehicleModMenu.AddItem(uiLstWheelType);
                        }
                    }

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
                            bool customWheels = GetVehicleModVariation(vehicle.Handle, 23);

                            SetVehicleMod(vehicle.Handle, (int)vehMod, variation, customWheels);
                            Logger.Debug($"Vehicle Mod: {vehMod}:{variation}");
                        };
                    }
                    else
                    {
                        Logger.Debug($"Vehicle Mod Ignored: {mod}");
                    }
                }

                vehicleModMenu.OnListChange += (sender, item, index) =>
                {
                    vehicle = Game.PlayerPed.CurrentVehicle;

                    if (item == uiLstWheelType)
                    {
                        // Needs work due to how NativeUI only returns current index

                        int vehicleClass = GetVehicleClass(vehicle.Handle);
                        bool isBikeOrOpenWheel = (index == 6 && vehicle.Model.IsBike) || (index == 10 && vehicleClass == 22);
                        bool isNotBikeNorOpenWheel = (index != 6 && !vehicle.Model.IsBike) && (index != 10 && vehicleClass != 22);
                        bool isCorrectVehicleType = (isBikeOrOpenWheel || isNotBikeNorOpenWheel);
                        //if (!isCorrectVehicleType)
                        //{
                        //    // Go past the index if it's not a bike.
                        //    if (!vehicle.Model.IsBike && vehicleClass != 22)
                        //    {
                        //        if (index > oldWheelIndex)
                        //        {
                        //            item.Index++;
                        //            oldWheelIndex = index;
                        //        }
                        //        else
                        //        {
                        //            item.Index--;
                        //        }
                        //    }
                        //    // Reset the index to 6 if it is a bike
                        //    else
                        //    {
                        //        item.Index = vehicle.Model.IsBike ? 6 : 10;
                        //    }
                        //}
                        // Set the wheel type
                        SetVehicleWheelType(vehicle.Handle, item.Index);

                        bool customWheels = GetVehicleModVariation(vehicle.Handle, 23);

                        // Reset the wheel mod index for front wheels
                        SetVehicleMod(vehicle.Handle, 23, -1, customWheels);

                        // If the model is a bike, do the same thing for the rear wheels.
                        if (vehicle.Model.IsBike)
                        {
                            SetVehicleMod(vehicle.Handle, 24, -1, customWheels);
                        }
                    }
                };

                int currentHeadlightColor = _GetHeadlightsColorFromVehicle(vehicle);
                if (currentHeadlightColor < 0 || currentHeadlightColor > 12)
                {
                    currentHeadlightColor = 13;
                }
                uiLstHeadlightColor.Index = currentHeadlightColor;
            }

            int currentTint = GetVehicleWindowTint(vehicle.Handle);
            if (currentTint == -1)
            {
                currentTint = 4;
            }

            switch (currentTint)
            {
                case 0:
                    currentTint = 1;
                    break;
                case 1:
                    currentTint = 5;
                    break;
                case 2:
                    currentTint = 4;
                    break;
                case 3:
                    currentTint = 3;
                    break;
                case 4:
                    currentTint = 0;
                    break;
                case 5:
                    currentTint = 2;
                    break;
                case 6:
                    currentTint = 6;
                    break;
            }

            uiLstWindowTint.Index = currentTint;

            uiChkBulletProofTires.Checked = !GetVehicleTyresCanBurst(vehicle.Handle);
            uiChkCustomWheels.Checked = IsToggleModOn(vehicle.Handle, 23);
            uiChkTurbo.Checked = IsToggleModOn(vehicle.Handle, 18);
            uiChkXenonHeadlights.Checked = IsToggleModOn(vehicle.Handle, 22);
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

        private readonly List<int[]> _VehicleNeonLightColors = new List<int[]>()
        {
            { new int[3] { 255, 255, 255 } },   // White
            { new int[3] { 2, 21, 255 } },      // Blue
            { new int[3] { 3, 83, 255 } },      // Electric blue
            { new int[3] { 0, 255, 140 } },     // Mint Green
            { new int[3] { 94, 255, 1 } },      // Lime Green
            { new int[3] { 255, 255, 0 } },     // Yellow
            { new int[3] { 255, 150, 5 } },     // Golden Shower
            { new int[3] { 255, 62, 0 } },      // Orange
            { new int[3] { 255, 0, 0 } },       // Red
            { new int[3] { 255, 50, 100 } },    // Pony Pink
            { new int[3] { 255, 5, 190 } },     // Hot Pink
            { new int[3] { 35, 1, 255 } },      // Purple
            { new int[3] { 15, 3, 255 } },      // Blacklight
        };

        private System.Drawing.Color GetColorFromIndex(int index)
        {
            if (index >= 0 && index < 13)
            {
                return System.Drawing.Color.FromArgb(_VehicleNeonLightColors[index][0], _VehicleNeonLightColors[index][1], _VehicleNeonLightColors[index][2]);
            }
            return System.Drawing.Color.FromArgb(255, 255, 255);
        }

        private int GetIndexFromColor()
        {
            Vehicle veh = Game.PlayerPed.CurrentVehicle;

            if (veh == null || !veh.Exists() || !veh.Mods.HasNeonLights)
            {
                return 0;
            }

            int r = 255, g = 255, b = 255;

            GetVehicleNeonLightsColour(veh.Handle, ref r, ref g, ref b);

            if (r == 255 && g == 0 && b == 255) // default return value when the vehicle has no neon kit selected.
            {
                return 0;
            }

            if (_VehicleNeonLightColors.Any(a => { return a[0] == r && a[1] == g && a[2] == b; }))
            {
                return _VehicleNeonLightColors.FindIndex(a => { return a[0] == r && a[1] == g && a[2] == b; });
            }

            return 0;
        }
    }
}
