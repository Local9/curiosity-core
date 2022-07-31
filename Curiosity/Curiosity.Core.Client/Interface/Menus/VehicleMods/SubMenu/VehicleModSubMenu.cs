using NativeUI;

namespace Curiosity.Core.Client.Interface.Menus.VehicleMods.SubMenu
{
    class VehicleModSubMenu
    {
        UIMenu menu;

        UIMenuListItem uiLstWheelType;
        Vehicle currentVehicle = null;

        bool hornTest = false;

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

        internal void Create(UIMenu vehicleModMenu)
        {
            menu = vehicleModMenu;
            menu.MouseControlsEnabled = false;

            menu.OnMenuStateChanged += (oldMenu, newMenu, state) =>
            {
                if (Equals(MenuState.Opened, state) || Equals(MenuState.ChangeForward, state))
                {
                    UpdateMods();
                    PluginManager.Instance.AttachTickHandler(TestHorn);
                }

                if (newMenu == menu && (state == MenuState.ChangeBackward || state == MenuState.Closed))
                {
                    PluginManager.Instance.DetachTickHandler(TestHorn);
                }
            };
        }

        private async Task TestHorn()
        {
            if (!Game.PlayerPed.IsInVehicle())
            {
                PluginManager.Instance.DetachTickHandler(TestHorn);
                return;
            }

            if (Game.IsControlJustPressed(0, Control.VehicleHorn))
            {
                Game.PlayerPed.CurrentVehicle.SoundHorn(3000);
            }
        }

        void UpdateMods()
        {
            // clear the menu

            // get the vehicle they are sat in
            Vehicle vehicle = Game.PlayerPed.CurrentVehicle;
            // check it
            if (vehicle is not null && vehicle.Exists() && !vehicle.IsDead && currentVehicle != vehicle)
            {
                menu.Clear();
                uiLstWheelType = null;

                currentVehicle = vehicle;

                // Set the vehicle mod kit
                vehicle.Mods.InstallModKit();
                // Get the vehicles mods
                VehicleMod[] vehicleMods = vehicle.Mods.GetAllMods();
                // loop over them
                foreach (VehicleMod mod in vehicleMods)
                {
                    if (mod.ModType == VehicleModType.Tank)
                    {
                        RemoveVehicleMod(vehicle.Handle, (int)VehicleModType.Tank);
                        continue;
                    }

                    if (mod.ModType == VehicleModType.Livery) continue;

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

                        List<object> lst = new();

                        lst.Add($"0/{mod.ModCount}");

                        for (var i = 1; i <= mod.ModCount; i++)
                        {
                            lst.Add($"{i}/{mod.ModCount}");
                        }

                        int currentValue = GetVehicleMod(vehicle.Handle, (int)mod.ModType);
                        if (currentValue == -1)
                            currentValue = 0;

                        UIMenuListItem uiListMod = new UIMenuListItem(name, lst, currentValue);
                        uiListMod.ItemData = new { mod = mod.ModType };
                        menu.AddItem(uiListMod);

                    }
                }

                menu.RefreshIndex();

                menu.OnListChange += (sender, item, index) =>
                {
                    vehicle = Game.PlayerPed.CurrentVehicle;
                    vehicle.Mods.InstallModKit();

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
                        return;
                    }

                    if (item.ItemData is not null)
                    {
                        var vehMod = item.ItemData?.mod;
                        var variation = index - 1; // remove the fake option we added
                        bool customWheels = GetVehicleModVariation(vehicle.Handle, 23);

                        if (variation == -1)
                        {
                            RemoveVehicleMod(vehicle.Handle, (int)vehMod);
                        }
                        else
                        {
                            SetVehicleMod(vehicle.Handle, (int)vehMod, variation, customWheels);
                        }

                        Logger.Debug($"Vehicle Mod: {vehMod}:{variation}");
                    }
                };
            }

        }
    }
}
