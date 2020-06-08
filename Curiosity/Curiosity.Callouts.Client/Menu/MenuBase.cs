using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Callouts.Client.Managers;
using Curiosity.Callouts.Client.Utils;
using Curiosity.Callouts.Shared.Classes;
using Curiosity.Callouts.Shared.EventWrapper;
using Curiosity.Callouts.Shared.Utils;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Callouts.Client.Menu
{
    class MenuBase : BaseScript
    {
        private PluginManager PluginInstance => PluginManager.Instance;
        public static MenuPool _MenuPool;
        private UIMenu menuMain;

        private bool isMenuOpen => Decorators.GetBoolean(Game.PlayerPed.Handle, Decorators.PLAYER_MENU);

        // sub menus
        private Submenu.Dispatch _dispatch = new Submenu.Dispatch();
        private Submenu.Suspect _suspect = new Submenu.Suspect();
        private UIMenu menuDispatch;
        private UIMenu menuSuspect;

        // menu items - Maybe move these???
        private UIMenuItem mItemRequestAssistance = new UIMenuItem($"Request Assistance", "Call for support during an active pursuit.");
        private UIMenuItem mItemPanicButton = new UIMenuItem($"Panic Button", "Call dispatch for immediate assistance.~n~~y~Will be removed after 2 mins~n~~o~5 Minute cooldown between requests");

        public MenuBase()
        {
            EventHandlers[Events.Native.Client.OnClientResourceStart.Path] += Events.Native.Client.OnClientResourceStart.Action +=
                resourceName =>
                {
                    if (resourceName != API.GetCurrentResourceName()) return;

                    Setup();
                };
        }

        void Setup()
        {
            _MenuPool = new MenuPool();
            _MenuPool.MouseEdgeEnabled = false;

            menuMain = new UIMenu("Activity Menu", "Options for your current activity");
            menuMain.MouseControlsEnabled = false;
            _MenuPool.Add(menuMain);

            menuMain.OnItemSelect += MenuMain_OnItemSelect;
            menuMain.OnMenuOpen += MenuMain_OnMenuOpen;
            menuMain.OnMenuClose += MenuMain_OnMenuClose;


            menuDispatch = _MenuPool.AddSubMenu(menuMain, "Dispatch", "Dispatch Options~n~~o~Options are available when a callout is active.");
            menuDispatch.MouseControlsEnabled = false;
            _dispatch.CreateMenu(menuDispatch);

            menuSuspect = _MenuPool.AddSubMenu(menuMain, "Suspect", "Suspect Options~n~~o~Options are available when a callout is active.");
            menuSuspect.MouseControlsEnabled = false;
            _suspect.CreateMenu(menuSuspect);

            menuMain.AddItem(mItemPanicButton);
            menuMain.AddItem(mItemRequestAssistance);

            _MenuPool.RefreshIndex();
        }

        private void MenuMain_OnMenuOpen(UIMenu sender)
        {
            OnMenuState(true);

            // Change button states
            bool isCalloutActive = CalloutManager.ActiveCallout != null;
            bool isPursuitActive = PursuitManager.IsPursuitActive;

            mItemRequestAssistance.Enabled = isPursuitActive;
            mItemRequestAssistance.Description = isPursuitActive ? "10-78 - Call dispatch for assistance." : "You'tr currently ~r~NOT~s~ in an active pursuit";
        }

        private async void MenuMain_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == mItemRequestAssistance)
            {
                PursuitManager.AddNewCopToPursuit();
            }

            if (selectedItem == mItemPanicButton)
            {
                PanicButton.Pressed();
            }
        }

        private void MenuMain_OnMenuClose(UIMenu sender)
        {
            OnMenuState();
        }

        private async Task OnMenuDisplay()
        {
            _MenuPool.ProcessMenus();
            _MenuPool.ProcessMouse();
        }

        // LEGACY
        public static void OnMenuState(bool isOpen = false)
        {
            _MenuPool.MouseEdgeEnabled = false;

            Decorators.Set(Game.PlayerPed.Handle, Decorators.PLAYER_MENU, isOpen);

            if (isOpen)
            {
                BaseScript.TriggerEvent("curiosity:Client:UI:LocationHide", true);
                BaseScript.TriggerEvent("curiosity:Client:Menu:IsOpened", true);
            }
            else
            {
                BaseScript.TriggerEvent("curiosity:Client:UI:LocationHide", false);
                BaseScript.TriggerEvent("curiosity:Client:Menu:IsOpened", false);
            }
        }

        [Tick]
        private async Task OnMenuControls()
        {
            if (Game.PlayerPed.IsAlive && PlayerManager.IsOfficer && !isMenuOpen)
            {
                if (Game.IsControlJustPressed(0, Control.ReplayStartStopRecording))
                {
                    PluginInstance.RegisterTickHandler(OnMenuDisplay);
                    menuMain.Visible = true;
                }
            }
        }
    }
}
