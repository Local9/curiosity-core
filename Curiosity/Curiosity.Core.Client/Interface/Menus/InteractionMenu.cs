using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Managers;
using Curiosity.Core.Client.Utils;
using Curiosity.Systems.Library.Models;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Interface.Menus
{
    public class InteractionMenu : Manager<InteractionMenu>
    {
        private const string PERSONAL_VEHICLE = "Personal Vehicle";
        private const string PERSONAL_BOAT = "Personal Boat";
        private const string PERSONAL_PLANE = "Personal Plane";
        private const string PERSONAL_TRAILER = "Personal Trailer";
        private const string PERSONAL_HELICOPTER = "Personal Helicopter";
        public static InteractionMenu MenuInstance;

        public static MenuPool MenuPool;
        private UIMenu menuMain;
        private int currentIndex;

        // menu items
        private static List<dynamic> gpsLocations = new List<dynamic>() { "Loading..." };
        private int gpsIndex = 0;
        private UIMenuListItem mlGpsLocations = new UIMenuListItem("GPS", gpsLocations, 0);

        // private UIMenuItem miPassive = new UIMenuItem("Enable Passive Mode", "Enabling passive mode will mean people cannot attack you, you will also be unable to use weapons.");
        private UIMenuItem miPassive = new UIMenuItem("Enable Passive Mode", "Enabling passive mode will mean people cannot attack you, you will also be unable to use weapons.");
        private UIMenuItem miKillYourself = new UIMenuItem("Kill Yourself", "Kill yourself and respawn.");
        private UIMenuItem miEditPed = new UIMenuItem("Customise Ped", "Change your look.");

        private UIMenu menuVehicle;
        private SubMenu.VehicleMenu _VehicleMenu = new SubMenu.VehicleMenu();

        public override void Begin()
        {
            MenuInstance = this;

            MenuPool = new MenuPool();
            MenuPool.MouseEdgeEnabled = false;

            menuMain = new UIMenu("Interaction Menu", "Player Interactions");
            menuMain.MouseControlsEnabled = false;

            menuMain.AddItem(mlGpsLocations);

            menuVehicle = MenuPool.AddSubMenu(menuMain, "Vehicles");
            _VehicleMenu.CreateMenu(menuVehicle);

            menuMain.AddItem(miKillYourself);
            menuMain.AddItem(miPassive);
            menuMain.AddItem(miEditPed);

            miKillYourself.SetRightLabel($"$0");

            menuMain.OnListChange += MenuMain_OnListChange;
            menuMain.OnListSelect += MenuMain_OnListSelect;
            menuMain.OnItemSelect += MenuMain_OnItemSelect;
            menuMain.OnIndexChange += MenuMain_OnIndexChange;

            menuMain.OnMenuStateChanged += MenuMain_OnMenuStateChanged;

            menuMain.RefreshIndex();

            MenuPool.Add(menuMain);
        }

        private void MenuMain_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            Logger.Debug($"Menu State: {state}");

            if (state == MenuState.Opened || state == MenuState.ChangeBackward || state == MenuState.ChangeForward)
                OnMenuOpen();

            if (state == MenuState.Closed)
                menuMain.InstructionalButtons.Clear();
        }

        private void MenuMain_OnIndexChange(UIMenu sender, int newIndex)
        {
            currentIndex = newIndex;
        }

        private async void MenuMain_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            PlayerOptionsManager playerOptionsManager = PlayerOptionsManager.GetModule();

            if (selectedItem == miPassive)
            {
                if (playerOptionsManager.IsPassiveModeEnabledCooldown) return;

                playerOptionsManager.SetPlayerPassive(!playerOptionsManager.IsPassiveModeEnabled);
                miPassive.Enabled = false;

                await BaseScript.Delay(1000);

                miPassive.Text = playerOptionsManager.IsPassiveModeEnabled ? "Disable Passive Mode" : "Enable Passive Mode";
                miPassive.Enabled = !playerOptionsManager.IsPassiveModeEnabledCooldown;

                string notificationText = (playerOptionsManager.IsPassiveModeEnabled) ? "Enabled" : "Disabled";
                Notify.Info($"Passive Mode: {notificationText}");
                return;
            }

            if (selectedItem == miKillYourself)
            {
                if (!playerOptionsManager.IsKillSelfEnabled) return;

                playerOptionsManager.KillSelf();
                miKillYourself.Enabled = false;
                return;
            }

            if (selectedItem == miEditPed)
            {
                Creator.CreatorMenus creatorMenus = new Creator.CreatorMenus();
                creatorMenus.CreateMenu(true);
                MenuPool.CloseAllMenus();
            }
        }

        private void MenuMain_OnListSelect(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (listItem == mlGpsLocations)
            {
                gpsIndex = newIndex;
                string key = (string)listItem.Items[newIndex];

                Logger.Debug($"Selected Key: {key}");

                if (key == PERSONAL_VEHICLE)
                {
                    PersonalVehicleWaypoint(Cache.PersonalVehicle.Vehicle);
                    return;
                }

                if (key == PERSONAL_TRAILER)
                {
                    PersonalVehicleWaypoint(Cache.PersonalTrailer.Vehicle);
                    return;
                }

                if (key == PERSONAL_PLANE)
                {
                    PersonalVehicleWaypoint(Cache.PersonalPlane.Vehicle);
                    return;
                }

                if (key == PERSONAL_HELICOPTER)
                {
                    PersonalVehicleWaypoint(Cache.PersonalHelicopter.Vehicle);
                    return;
                }

                if (key == PERSONAL_BOAT)
                {
                    PersonalVehicleWaypoint(Cache.PersonalBoat.Vehicle);
                    return;
                }

                List<Position> positions = BlipManager.ManagerInstance.Locations[key];
                List<Vector3> posVectors = new List<Vector3>();
                positions.ForEach(x => posVectors.Add(x.AsVector()));

                Vector3 closestPosition = Cache.PlayerPed.Position.FindClosestPoint(posVectors);

                if (closestPosition.Equals(Vector3.Zero)) return;

                API.SetNewWaypoint(closestPosition.X, closestPosition.Y);
            }
        }

        private void PersonalVehicleWaypoint(Vehicle vehicle)
        {
            if (vehicle is null) return;
            if (!(vehicle?.Exists() ?? false)) return;

            Vector3 pos = vehicle.Position;
            API.SetNewWaypoint(pos.X, pos.Y);
        }

        private void MenuMain_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (listItem == mlGpsLocations)
            {
                gpsIndex = newIndex;
            }
        }

        private void OnMenuOpen()
        {
            menuMain.InstructionalButtons.Clear();

            PlayerOptionsManager playerOptionsManager = PlayerOptionsManager.GetModule();

            Logger.Debug($"Menu Open");
            // TOP
            UpdateGpsMenuItem(true);
            // MID

            // BOTTOM
            miPassive.Text = playerOptionsManager.IsPassiveModeEnabled ? "Disable Passive Mode" : "Enable Passive Mode";
            miPassive.Enabled = !playerOptionsManager.IsPassiveModeEnabledCooldown;

            miKillYourself.Enabled = playerOptionsManager.IsKillSelfEnabled;
            miKillYourself.SetRightLabel($"${playerOptionsManager.CostOfKillSelf * playerOptionsManager.NumberOfTimesKillSelf}");
        }

        private async Task OnMenuDisplay()
        {
            MenuPool.ProcessMenus();
            MenuPool.ProcessMouse();

            MenuPool.MouseEdgeEnabled = false;
        }

        [TickHandler(SessionWait = true)]
        private async Task OnMenuControls()
        {
            try
            {
                while (Cache.Character == null)
                {
                    await BaseScript.Delay(100);
                }

                if (!Game.IsPaused && !API.IsPauseMenuRestarting() && API.IsScreenFadedIn() && !API.IsPlayerSwitchInProgress() && Cache.Character.MarkedAsRegistered)
                {
                    if (MenuPool.IsAnyMenuOpen())
                    {
                        if (Game.CurrentInputMode == InputMode.MouseAndKeyboard)
                        {
                            if ((Game.IsControlJustPressed(0, Control.InteractionMenu) || Game.IsDisabledControlJustPressed(0, Control.InteractionMenu)))
                            {
                                menuMain.Visible = false;
                                MenuPool.CloseAllMenus();
                            }
                        }
                    }
                    else
                    {
                        if (Game.CurrentInputMode == InputMode.GamePad)
                        {
                            int tmpTimer = API.GetGameTimer();

                            while ((Game.IsControlPressed(0, Control.InteractionMenu) || Game.IsDisabledControlPressed(0, Control.InteractionMenu)) && !Game.IsPaused && API.IsScreenFadedIn() && !API.IsPlayerSwitchInProgress())
                            {
                                if (API.GetGameTimer() - tmpTimer > 400)
                                {
                                    if (!MenuPool.IsAnyMenuOpen())
                                    {
                                        Instance.AttachTickHandler(OnMenuDisplay);
                                        menuMain.Visible = !menuMain.Visible;
                                    }
                                    break;
                                }
                                await BaseScript.Delay(0);
                            }
                        }
                        else
                        {
                            if ((Game.IsControlJustPressed(0, Control.InteractionMenu) || Game.IsDisabledControlJustPressed(0, Control.InteractionMenu)) && !Game.IsPaused && API.IsScreenFadedIn() && !API.IsPlayerSwitchInProgress())
                            {
                                if (!MenuPool.IsAnyMenuOpen())
                                {
                                    Instance.AttachTickHandler(OnMenuDisplay);
                                    menuMain.Visible = !menuMain.Visible;
                                }
                            }
                        }
                    }
                }

                if (menuMain is not null)
                {
                    if (!menuMain.Visible)
                    {
                        if (menuMain.InstructionalButtons.Count > 0)
                            menuMain.InstructionalButtons.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "On Menu Open Tick");
            }
        }

        // Menu Items
        public void UpdateGpsMenuItem(bool reset = false)
        {
            if (reset)
            {
                gpsLocations.Clear();
            }

            if (gpsLocations.Count > 0) return;

            foreach (KeyValuePair<string, List<Position>> kvp in BlipManager.ManagerInstance.Locations)
            {
                if (!gpsLocations.Contains(kvp.Key))
                    gpsLocations.Add(kvp.Key);
            }

            if (!gpsLocations.Contains(PERSONAL_VEHICLE))
            {
                if (Cache.PersonalVehicle is not null)
                {
                    Vehicle vehicle = Cache.PersonalVehicle.Vehicle;
                    if (vehicle?.Exists() ?? false)
                    {
                        gpsLocations.Add(PERSONAL_VEHICLE);
                    }
                }
            }

            if (!gpsLocations.Contains(PERSONAL_BOAT))
            {
                if (Cache.PersonalBoat is not null)
                {
                    Vehicle vehicle = Cache.PersonalBoat.Vehicle;
                    if (vehicle?.Exists() ?? false)
                    {
                        gpsLocations.Add(PERSONAL_BOAT);
                    }
                }
            }

            if (!gpsLocations.Contains(PERSONAL_HELICOPTER))
            {
                if (Cache.PersonalHelicopter is not null)
                {
                    Vehicle vehicle = Cache.PersonalHelicopter.Vehicle;
                    if (vehicle?.Exists() ?? false)
                    {
                        gpsLocations.Add(PERSONAL_HELICOPTER);
                    }
                }
            }

            if (!gpsLocations.Contains(PERSONAL_PLANE))
            {
                if (Cache.PersonalPlane is not null)
                {
                    Vehicle vehicle = Cache.PersonalPlane.Vehicle;
                    if (vehicle?.Exists() ?? false)
                    {
                        gpsLocations.Add(PERSONAL_PLANE);
                    }
                }
            }

            if (!gpsLocations.Contains(PERSONAL_TRAILER))
            {
                if (Cache.PersonalTrailer is not null)
                {
                    Vehicle vehicle = Cache.PersonalTrailer.Vehicle;
                    if (vehicle?.Exists() ?? false)
                    {
                        gpsLocations.Add(PERSONAL_TRAILER);
                    }
                }
            }

            if (gpsLocations.Count > 1)
            {
                gpsLocations.Sort((x, y) => string.Compare(x, y));

                mlGpsLocations.Items = gpsLocations;
                mlGpsLocations.Index = gpsIndex;
            }
        }
    }
}
