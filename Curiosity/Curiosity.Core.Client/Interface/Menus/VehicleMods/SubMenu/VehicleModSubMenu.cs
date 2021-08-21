using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using NativeUI;
using System.Collections.Generic;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Interface.Menus.VehicleMods.SubMenu
{
    class VehicleModSubMenu
    {
        UIMenu menu;

        UIMenuListItem uiLstWheelType;

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

        private int baseMenuIndex = 0;

        internal void Create(UIMenu vehicleModMenu)
        {
            menu = vehicleModMenu;
            menu.MouseControlsEnabled = false;

            menu.OnIndexChange += (sender, newIndex) =>
            {
                baseMenuIndex = newIndex;
            };

            menu.OnMenuStateChanged += (oldMenu, newMenu, state) =>
            {
                if (Equals(MenuState.Opened, state) || Equals(MenuState.ChangeForward, state))
                {
                    UpdateMods();
                }

                if (newMenu == menu && state == MenuState.ChangeBackward)
                {
                    menu.CurrentSelection = baseMenuIndex;
                }
            };
        }

        void UpdateMods()
        {
            // clear the menu
            menu.Clear();
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
                foreach (VehicleMod mod in vehicleMods)
                {
                    if (mod.ModType == VehicleModType.FrontWheel && uiLstWheelType is null)
                    {
                        if (!vehicle.Model.IsBoat && !vehicle.Model.IsHelicopter && !vehicle.Model.IsPlane && !vehicle.Model.IsBicycle && !vehicle.Model.IsTrain)
                        {
                            uiLstWheelType = new UIMenuListItem("Wheel Type", wheelTypes, MathUtil.Clamp(GetVehicleWheelType(vehicle.Handle), 0, 11));
                            menu.AddItem(uiLstWheelType);
                        }
                    }

                    if (mod.ModCount > 0)
                    {
                        string name = mod.LocalizedModTypeName;
                        UIMenu modSubmenu = VehicleModMenu._MenuPool.AddSubMenu(menu, $"{name}");

                        for (var i = 0; i < mod.ModCount; i++)
                        {
                            UIMenuItem uIMenuItem = new UIMenuItem($"{name} #{i + 1}");
                            uIMenuItem.ItemData = new { mod = mod.ModType, variation = i };
                            modSubmenu.AddItem(uIMenuItem);
                        }

                        modSubmenu.OnItemSelect += (sender, item, index) => {
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

                menu.OnListChange += (sender, item, index) =>
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

                //int currentHeadlightColor = _GetHeadlightsColorFromVehicle(vehicle);
                //if (currentHeadlightColor < 0 || currentHeadlightColor > 12)
                //{
                //    currentHeadlightColor = 13;
                //}
                //uiLstHeadlightColor.Index = currentHeadlightColor;
            }

            //int currentTint = GetVehicleWindowTint(vehicle.Handle);
            //if (currentTint == -1)
            //{
            //    currentTint = 4;
            //}

            //switch (currentTint)
            //{
            //    case 0:
            //        currentTint = 1;
            //        break;
            //    case 1:
            //        currentTint = 5;
            //        break;
            //    case 2:
            //        currentTint = 4;
            //        break;
            //    case 3:
            //        currentTint = 3;
            //        break;
            //    case 4:
            //        currentTint = 0;
            //        break;
            //    case 5:
            //        currentTint = 2;
            //        break;
            //    case 6:
            //        currentTint = 6;
            //        break;
            //}

            //uiLstWindowTint.Index = currentTint;
        }
    }
}
