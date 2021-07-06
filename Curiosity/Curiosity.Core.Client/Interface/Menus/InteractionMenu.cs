﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Managers;
using Curiosity.Core.Client.Utils;
using Curiosity.Systems.Library.Models;
using NativeUI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Interface.Menus
{
    public class InteractionMenu : Manager<InteractionMenu>
    {
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

            MenuPool.Add(menuMain);

            menuMain.OnListChange += MenuMain_OnListChange;
            menuMain.OnListSelect += MenuMain_OnListSelect;
            menuMain.OnItemSelect += MenuMain_OnItemSelect;
            menuMain.OnIndexChange += MenuMain_OnIndexChange;

            menuMain.OnMenuStateChanged += MenuMain_OnMenuStateChanged;

            menuMain.RefreshIndex();
        }

        private void MenuMain_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.Opened)
                OnMenuOpen();
        }

        private void MenuMain_OnIndexChange(UIMenu sender, int newIndex)
        {
            currentIndex = newIndex;
        }

        private async void MenuMain_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == miPassive)
            {
                if (PlayerOptions.IsPassiveModeEnabledCooldown) return;

                Cache.Player.User.IsPassive = !Cache.Player.User.IsPassive;
                PlayerOptions.SetPlayerPassive(Cache.Player.User.IsPassive);
                miPassive.Enabled = false;

                await BaseScript.Delay(1000);

                miPassive.Text = (Cache.Player.User.IsPassive) ? "Disable Passive Mode" : "Enable Passive Mode";
                miPassive.Enabled = PlayerOptions.IsPassiveModeEnabled;

                string notificationText = (Cache.Player.User.IsPassive) ? "Enabled" : "Disabled";
                Notify.Info($"Passive Mode: {notificationText}");
                return;
            }

            if (selectedItem == miKillYourself)
            {
                if (!PlayerOptions.IsKillSelfEnabled) return;

                PlayerOptions.KillSelf();
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
            // TOP
            UpdateGpsMenuItem();
            // MID

            // BOTTOM
            miPassive.Text = (Cache.Player.User.IsPassive) ? "Disable Passive Mode" : "Enable Passive Mode";
            miPassive.Enabled = !PlayerOptions.IsPassiveModeEnabledCooldown;

            miKillYourself.Enabled = PlayerOptions.IsKillSelfEnabled;
            miKillYourself.SetRightLabel($"${PlayerOptions.CostOfKillSelf * PlayerOptions.NumberOfTimesKillSelf}");
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

            if (gpsLocations.Count > 1)
            {
                gpsLocations.Sort((x, y) => string.Compare(x, y));

                mlGpsLocations.Items = gpsLocations;
                mlGpsLocations.Index = gpsIndex;
            }
        }
    }
}
