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
                    if (Cache.PersonalVehicle is null) return;
                    if (!(Cache.PersonalVehicle.Vehicle?.Exists() ?? false)) return;

                    Vector3 pos = Cache.PersonalVehicle.Vehicle.Position;
                    API.SetNewWaypoint(pos.X, pos.Y);
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

            if (gpsLocations.Count > 1)
            {
                gpsLocations.Sort((x, y) => string.Compare(x, y));

                mlGpsLocations.Items = gpsLocations;
                mlGpsLocations.Index = gpsIndex;
            }
        }
    }
}
