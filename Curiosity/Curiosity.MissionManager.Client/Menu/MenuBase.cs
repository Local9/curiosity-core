using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.EventWrapper;
using Curiosity.MissionManager.Client.Environment;
using Curiosity.MissionManager.Client.Utils;
using NativeUI;
using System.Linq;
using System.Threading.Tasks;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.MissionManager.Client.Menu
{
    class MenuBase : BaseScript
    {
        private PluginManager PluginInstance => PluginManager.Instance;
        public static MenuPool _MenuPool;
        private UIMenu menuMain;
        public static bool IsCalloutActive;

        private bool isMenuOpen => Decorators.GetBoolean(Game.PlayerPed.Handle, Decorators.PLAYER_MENU);

        // sub menus
        private Submenu.Dispatch _dispatch = new Submenu.Dispatch();
        private Submenu.Suspect _suspect = new Submenu.Suspect();
        private UIMenu menuDispatch;
        private UIMenu menuSuspect;

        // menu items - Maybe move these???
        private UIMenuItem mItemRequestAssistance = new UIMenuItem($"Request Assistance", "Call for support during an active pursuit."); // Call players?

        public MenuBase()
        {
            EventHandlers[Events.Native.Client.OnClientResourceStart.Path] += Events.Native.Client.OnClientResourceStart.Action +=
                resourceName =>
                {
                    if (resourceName != API.GetCurrentResourceName()) return;

                    Decorators.Set(Game.PlayerPed.Handle, Decorators.PLAYER_MENU, false);

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

            _MenuPool.RefreshIndex();
        }

        private void MenuMain_OnMenuOpen(UIMenu sender)
        {
            OnMenuState(true);
            IsCalloutActive = Mission.isOnMission;
        }

        private async void MenuMain_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {

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
            if (Game.PlayerPed.IsAlive && CurPlayer.IsOfficer && !isMenuOpen)
            {
                if (Game.IsControlJustPressed(0, Control.ReplayStartStopRecording)) // F2
                {
                    PluginInstance.RegisterTickHandler(OnMenuDisplay);
                    menuMain.Visible = true;
                }
            }
        }

        public static Ped GetClosestSuspect()
        {
            if (!IsCalloutActive) return null;

            return Mission.RegisteredPeds.Select(x => x).Where(p => p.Position.Distance(Game.PlayerPed.Position) < 2f && p.IsSuspect && p.IsMission).FirstOrDefault();
        }

        public static Vehicle GetClosestVehicle()
        {
            if (!IsCalloutActive) return null;

            return Mission.RegisteredVehicles.Select(x => x).Where(p => p.Position.Distance(Game.PlayerPed.Position) < 4f && p.IsMission).FirstOrDefault();
        }
    }
}
