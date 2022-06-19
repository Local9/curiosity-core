using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface.Menus.VehicleMods.SubMenu;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Models;
using NativeUI;
using System.Linq;

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
        private UIMenu vehicleExtraMenu;
        private VehicleExtrasSubMenu vehicleExtrasSubMenu = new VehicleExtrasSubMenu();
        private UIMenu vehicleLiveriesMenu;
        private VehicleLiveriesSubMenu vehicleLiveriesSubMenu = new VehicleLiveriesSubMenu();

        private UIMenuCheckboxItem uiChkXenonHeadlights;
        private UIMenuCheckboxItem uiChkTurbo;
        private UIMenuCheckboxItem uiChkBulletProofTires;
        private UIMenuCheckboxItem uiChkCustomWheels;
        private UIMenuCheckboxItem uiChkTireSmoke;

        private UIMenuListItem uiLstWindowTint;
        private UIMenuListItem uiLstHeadlightColor;
        private UIMenuListItem uiLstTireSmoke;

        private UIMenuItem miSaveVehicle;
        private UIMenuItem miCloseMenu;

        int oldWheelIndex = 0;

        List<dynamic> headlightColor = new List<dynamic>() { "White", "Blue", "Electric Blue", "Mint Green", "Lime Green", "Yellow", "Golden Shower", "Orange", "Red", "Pony Pink", "Hot Pink", "Purple", "Blacklight", "Default Xenon" };
        List<dynamic> windowTints = new List<dynamic>() { "Stock", "None", "Limo", "Light Smoke", "Dark Smoke", "Pure Black", "Green" };

        List<dynamic> tireSmokes = new List<dynamic>() { "Red", "Orange", "Yellow", "Gold", "Light Green", "Dark Green", "Light Blue", "Dark Blue", "Purple", "Pink", "Black" };
        Dictionary<string, int[]> tireSmokeColors = new Dictionary<string, int[]>()
        {
            ["Red"] = new int[] { 244, 65, 65 },
            ["Orange"] = new int[] { 244, 167, 66 },
            ["Yellow"] = new int[] { 244, 217, 65 },
            ["Gold"] = new int[] { 181, 120, 0 },
            ["Light Green"] = new int[] { 158, 255, 84 },
            ["Dark Green"] = new int[] { 44, 94, 5 },
            ["Light Blue"] = new int[] { 65, 211, 244 },
            ["Dark Blue"] = new int[] { 24, 54, 163 },
            ["Purple"] = new int[] { 108, 24, 192 },
            ["Pink"] = new int[] { 192, 24, 172 },
            ["Black"] = new int[] { 1, 1, 1 }
        };

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

            vehicleExtraMenu = _MenuPool.AddSubMenu(mainMenu, "Extras");
            vehicleExtrasSubMenu.Create(vehicleExtraMenu);

            vehicleLiveriesMenu = _MenuPool.AddSubMenu(mainMenu, "Liveries");
            vehicleLiveriesSubMenu.Create(vehicleLiveriesMenu);

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

            uiChkTireSmoke = new UIMenuCheckboxItem("Tire Smoke", false);
            uiLstTireSmoke = new UIMenuListItem("Smoke Color", tireSmokes, 0);
            mainMenu.AddItem(uiChkTireSmoke);
            mainMenu.AddItem(uiLstTireSmoke);

            miSaveVehicle = new UIMenuItem("Save", "~s~This will cost you ~g~$5000~s~ to save.");
            mainMenu.AddItem(miSaveVehicle);

            miCloseMenu = new UIMenuItem("Close");
            mainMenu.AddItem(miCloseMenu);

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
            else if (checkboxItem == uiChkTireSmoke)
            {
                if (Checked)
                {
                    // Enable it.
                    ToggleVehicleMod(vehicle.Handle, 20, true);
                    // Get the selected color values.
                    var r = tireSmokeColors[tireSmokes[uiLstTireSmoke.Index]][0];
                    var g = tireSmokeColors[tireSmokes[uiLstTireSmoke.Index]][1];
                    var b = tireSmokeColors[tireSmokes[uiLstTireSmoke.Index]][2];
                    // Set the color.
                    SetVehicleTyreSmokeColor(vehicle.Handle, r, g, b);
                }
                // If it should be disabled:
                else
                {
                    // Set the smoke to white.
                    SetVehicleTyreSmokeColor(vehicle.Handle, 255, 255, 255);
                    // Disable it.
                    ToggleVehicleMod(vehicle.Handle, 20, false);
                    // Remove the mod.
                    RemoveVehicleMod(vehicle.Handle, 20);
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
                    vehicle.SetHeadlightsColorOnVehicle(255);
                }
                else if (newIndex > -1 && newIndex < 13)
                {
                    vehicle.SetHeadlightsColorOnVehicle(newIndex);
                }
            }
            else if (listItem == uiLstTireSmoke)
            {
                // Get the selected color values.
                var r = tireSmokeColors[tireSmokes[newIndex]][0];
                var g = tireSmokeColors[tireSmokes[newIndex]][1];
                var b = tireSmokeColors[tireSmokes[newIndex]][2];

                // Set the color.
                SetVehicleTyreSmokeColor(vehicle.Handle, r, g, b);
            }
        }

        private void MainMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == miCloseMenu)
            {
                CloseModMenu();
            }
            else if (selectedItem == miSaveVehicle)
            {
                SaveVehicle();
            }
        }

        private void CloseModMenu()
        {
            Instance.DetachTickHandler(OnMenuCreate);

            mainMenu.InstructionalButtons.Clear();

            if (mainMenu.Visible)
                mainMenu.Visible = false;
        }

        private async void SaveVehicle()
        {
            if (Game.PlayerPed.IsInVehicle())
            {
                // Get the vehicle.
                Vehicle veh = Game.PlayerPed.CurrentVehicle;

                // Make sure the entity is actually a vehicle and it still exists, and it's not dead.
                if (veh != null && veh.Exists() && !veh.IsDead && veh.IsDriveable)
                {
                    Dictionary<int, int> mods = new Dictionary<int, int>();

                    foreach (var mod in veh.Mods.GetAllMods())
                    {
                        mods.Add((int)mod.ModType, mod.Index);
                    }

                    #region colors
                    var colors = new Dictionary<string, int>();
                    int primaryColor = 0;
                    int secondaryColor = 0;
                    int pearlescentColor = 0;
                    int wheelColor = 0;
                    int dashColor = 0;
                    int trimColor = 0;
                    GetVehicleExtraColours(veh.Handle, ref pearlescentColor, ref wheelColor);
                    GetVehicleColours(veh.Handle, ref primaryColor, ref secondaryColor);
                    GetVehicleDashboardColour(veh.Handle, ref dashColor);
                    GetVehicleInteriorColour(veh.Handle, ref trimColor);
                    colors.Add("primary", primaryColor);
                    colors.Add("secondary", secondaryColor);
                    colors.Add("pearlescent", pearlescentColor);
                    colors.Add("wheels", wheelColor);
                    colors.Add("dash", dashColor);
                    colors.Add("trim", trimColor);
                    int neonR = 255;
                    int neonG = 255;
                    int neonB = 255;
                    if (veh.Mods.HasNeonLights)
                    {
                        GetVehicleNeonLightsColour(veh.Handle, ref neonR, ref neonG, ref neonB);
                    }
                    colors.Add("neonR", neonR);
                    colors.Add("neonG", neonG);
                    colors.Add("neonB", neonB);
                    int tyresmokeR = 0;
                    int tyresmokeG = 0;
                    int tyresmokeB = 0;
                    GetVehicleTyreSmokeColor(veh.Handle, ref tyresmokeR, ref tyresmokeG, ref tyresmokeB);
                    colors.Add("tyresmokeR", tyresmokeR);
                    colors.Add("tyresmokeG", tyresmokeG);
                    colors.Add("tyresmokeB", tyresmokeB);
                    #endregion

                    var extras = new Dictionary<int, bool>();
                    for (int i = 0; i < 20; i++)
                    {
                        if (veh.ExtraExists(i))
                        {
                            extras.Add(i, veh.IsExtraOn(i));
                        }
                    }

                    VehicleInfo vi = new VehicleInfo()
                    {
                        colors = colors,
                        customWheels = GetVehicleModVariation(veh.Handle, 23),
                        extras = extras,
                        livery = GetVehicleLivery(veh.Handle),
                        model = (uint)GetEntityModel(veh.Handle),
                        mods = mods,
                        name = GetLabelText(GetDisplayNameFromVehicleModel((uint)GetEntityModel(veh.Handle))),
                        neonBack = veh.Mods.IsNeonLightsOn(VehicleNeonLight.Back),
                        neonFront = veh.Mods.IsNeonLightsOn(VehicleNeonLight.Front),
                        neonLeft = veh.Mods.IsNeonLightsOn(VehicleNeonLight.Left),
                        neonRight = veh.Mods.IsNeonLightsOn(VehicleNeonLight.Right),
                        plateText = veh.Mods.LicensePlate,
                        plateStyle = (int)veh.Mods.LicensePlateStyle,
                        turbo = IsToggleModOn(veh.Handle, 18),
                        tyreSmoke = IsToggleModOn(veh.Handle, 20),
                        version = 1,
                        wheelType = GetVehicleWheelType(veh.Handle),
                        windowTint = (int)veh.Mods.WindowTint,
                        xenonHeadlights = IsToggleModOn(veh.Handle, 22),
                        bulletProofTires = !veh.CanTiresBurst,
                        headlightColor = veh.GetHeadlightsColorFromVehicle(),
                        enveffScale = GetVehicleEnveffScale(veh.Handle)
                    };

                    ExportMessage exportMessage = await EventSystem.Request<ExportMessage>("garage:save", veh.NetworkId, vi);

                    if (exportMessage.success)
                    {
                        NotificationManager.GetModule().Success($"Vehicle has been saved: $5,000");
                        CloseModMenu();
                    }
                    else
                    {
                        NotificationManager.GetModule().Error($"{exportMessage.error}");
                    }
                };
            }
        }

        public void OpenMenu()
        {
            if (!Game.PlayerPed.IsInVehicle())
            {
                NotificationManager.GetModule().Error($"You're not in a vehicle.");
                return;
            }

            if (Game.PlayerPed.IsInVehicle() && Game.PlayerPed.CurrentVehicle.Driver != Game.PlayerPed)
            {
                NotificationManager.GetModule().Error($"You're not the driver.");
                return;
            }

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
                Vehicle veh = Game.PlayerPed.CurrentVehicle;
                int smoker = 0, smokeg = 0, smokeb = 0;
                GetVehicleTyreSmokeColor(veh.Handle, ref smoker, ref smokeg, ref smokeb);
                var item = tireSmokeColors.ToList().Find((f) => { return (f.Value[0] == smoker && f.Value[1] == smokeg && f.Value[2] == smokeb); });
                int index = tireSmokeColors.ToList().IndexOf(item);
                if (index < 0)
                {
                    index = 0;
                }

                uiLstTireSmoke.Index = index;

                uiChkTireSmoke.Checked = IsToggleModOn(veh.Handle, 20);
                uiChkBulletProofTires.Checked = !GetVehicleTyresCanBurst(veh.Handle);
                uiChkCustomWheels.Checked = IsToggleModOn(veh.Handle, 23);
                uiChkTurbo.Checked = IsToggleModOn(veh.Handle, 18);
                uiChkXenonHeadlights.Checked = IsToggleModOn(veh.Handle, 22);
            }
        }
    }
}
