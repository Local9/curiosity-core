using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Environment.Enums;
using Curiosity.MissionManager.Client.Managers;
using Curiosity.MissionManager.Client.Utils;
using NativeUI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Curiosity.Systems.Library.EventWrapperLegacy.LegacyEvents;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.MissionManager.Client.Menu
{
    public class MenuManager : Manager<MenuManager>
    {
        public static MenuPool _MenuPool;
        internal static UIMenu menuMain;
        public static bool IsCalloutActive = false;

        private static bool isMenuOpen
        {
            get
            {
                return Decorators.GetBoolean(Game.PlayerPed.Handle, Decorators.PLAYER_MENU);
            }
            set
            {
                Decorators.Set(Game.PlayerPed.Handle, Decorators.PLAYER_MENU, value);
            }

        }

        // sub menus
        internal static Submenu.MenuDispatch _dispatch = new Submenu.MenuDispatch();
        private Submenu.MenuSuspect _suspectPed = new Submenu.MenuSuspect();
        private Submenu.MenuVehicle _suspectVehicle = new Submenu.MenuVehicle();
        private Submenu.MenuSettings _menuSettings = new Submenu.MenuSettings();
        internal static UIMenu menuDispatch;
        private UIMenu menuSuspectPed;
        private UIMenu menuSuspectVehicle;
        private UIMenu menuSettings;

        public override void Begin()
        {
            Logger.Info($"- [MenuManager] Begin ----------------------------");

            Instance.EventRegistry[Native.Client.OnClientResourceStart.Path] += Native.Client.OnClientResourceStart.Action +=
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
            menuMain.OnMenuChange += MenuMain_OnMenuChange;

            menuDispatch = _MenuPool.AddSubMenu(menuMain, "Dispatch", "Dispatch Options~n~~o~Options are available when a callout is active.");
            menuDispatch.MouseControlsEnabled = false;
            _dispatch.CreateMenu(menuDispatch);

            menuSuspectPed = _MenuPool.AddSubMenu(menuMain, "Suspect", "Suspect Options~n~~o~Options are available when a callout is active.");
            menuSuspectPed.MouseControlsEnabled = false;
            _suspectPed.CreateMenu(menuSuspectPed);

            menuSuspectVehicle = _MenuPool.AddSubMenu(menuMain, "Vehicle", "Suspect Vehicle Options~n~~o~Options are available when a callout is active.");
            menuSuspectVehicle.MouseControlsEnabled = false;
            _suspectVehicle.CreateMenu(menuSuspectVehicle);

            menuSettings = _MenuPool.AddSubMenu(menuMain, "Settings", "General Options.");
            menuSettings.MouseControlsEnabled = false;
            _menuSettings.CreateMenu(menuSettings);

            _MenuPool.RefreshIndex();
        }

        private void MenuMain_OnMenuChange(UIMenu oldMenu, UIMenu newMenu, bool forward)
        {
            
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
            isMenuOpen = false;
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
            isMenuOpen = isOpen;
            BaseScript.TriggerEvent("curiosity:Client:Menu:IsOpened", isMenuOpen);
        }

        [TickHandler]
        private async Task OnMenuControls()
        {
            if (!JobManager.IsOfficer) return; // no point in showing if their're not an officer

            if (MarkerManager.GetActiveMarker(MarkerFilter.Unknown) != null) return; // hide base menu prompt if near a marker
            
            if (Game.PlayerPed.IsInVehicle() && Game.PlayerPed?.CurrentVehicle?.Speed > 4f) return; // driving? hide it

            List<CitizenFX.Core.Ped> peds = World.GetAllPeds().Where(x => x.IsInRangeOf(Game.PlayerPed.Position, 2f) && Decorators.GetBoolean(x.Handle, Decorators.PED_MISSION)).Select(p => p).ToList();
            List<CitizenFX.Core.Vehicle> vehicles = World.GetAllVehicles().Where(x => x.IsInRangeOf(Game.PlayerPed.Position, 4f)
                && (Decorators.GetBoolean(x.Handle, Decorators.VEHICLE_MISSION)
                || (Decorators.GetBoolean(x.Handle, Decorators.PLAYER_VEHICLE) && Decorators.GetInteger(x.Handle, Decorators.PLAYER_OWNER) == Game.Player.ServerId))
                ).Select(p => p).ToList();

            int interactables = peds.Count + vehicles.Count; // near any interactives?

            if (interactables == 0)
            {
                await BaseScript.Delay(1000);
                return;
            }

            Screen.DisplayHelpTextThisFrame($"Press ~INPUT_REPLAY_START_STOP_RECORDING~ to open menu."); // need to look into control binds

            if (Game.PlayerPed.IsAlive && JobManager.IsOfficer && !isMenuOpen)
            {
                if (Game.IsControlJustPressed(0, Control.ReplayStartStopRecording)) // F1
                {
                    if (menuMain.Visible) return;

                    if (!menuMain.Visible)
                    {
                        menuMain.Visible = true;
                        isMenuOpen = true;
                        Instance.AttachTickHandler(OnMenuDisplay);
                    }
                }
            }
        }

        public static Ped GetClosestInteractivePed()
        {
            if (!IsCalloutActive) return null;

            return Mission.RegisteredPeds.Select(x => x).Where(p => p.IsInRangeOf(Game.PlayerPed.Position, 2f) && p.IsMission).FirstOrDefault();
        }

        public static Vehicle GetClosestVehicle()
        {
            if (!IsCalloutActive) return null;

            return Mission.RegisteredVehicles.Select(x => x).Where(p => p.IsInRangeOf(Game.PlayerPed.Position, 4f) && p.IsMission).FirstOrDefault();
        }
    }
}
