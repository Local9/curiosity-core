using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Environment.Enums;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.MissionManager.Client.Manager;
using Curiosity.MissionManager.Client.Managers;
using Curiosity.MissionManager.Client.Utils;
using NativeUI;
using System;
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
        private const string COMMAND_OPEN_MENU = "open_police_interaction_menu";
        public static MenuPool _MenuPool;
        internal static UIMenu menuMain;
        public static bool HasShownWarning = false;
        public static bool CanShowMessage = true;

        // sub menus
        internal static Submenu.MenuDispatch _dispatch = new Submenu.MenuDispatch();
        private Submenu.MenuSuspect _suspectPed = new Submenu.MenuSuspect();
        private Submenu.MenuVehicle _suspectVehicle = new Submenu.MenuVehicle();
        private Submenu.MenuSettings _menuSettings = new Submenu.MenuSettings();
        private Submenu.MenuAssistanceRequesters _menuAssitianceRequesters = new Submenu.MenuAssistanceRequesters();
        private UIMenu menuDispatch;
        private UIMenu menuSuspectPed;
        private UIMenu menuSuspectVehicle;
        private UIMenu menuSettings;

        private UIMenu menuAssistanceRequesters;

        public override void Begin()
        {
            Logger.Info($"- [MenuManager] Begin ----------------------------");

            Instance.EventRegistry[Native.Client.OnClientResourceStart.Path] += Native.Client.OnClientResourceStart.Action +=
                resourceName =>
                {
                    if (resourceName != API.GetCurrentResourceName()) return;

                    Decorators.Set(API.PlayerPedId(), Decorators.PLAYER_MENU, false);

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

            menuMain.OnMenuStateChanged += MenuMain_OnMenuStateChanged;

            menuDispatch = _MenuPool.AddSubMenu(menuMain, "Dispatch", "Dispatch Options~n~~o~Options are available when a callout is active.");
            menuDispatch.MouseControlsEnabled = false;
            _dispatch.CreateMenu(menuDispatch);

            menuAssistanceRequesters = _MenuPool.AddSubMenu(menuMain, "Respond to Backup", "Users requesting back up will be found here.");
            menuAssistanceRequesters.MouseControlsEnabled = false;
            _menuAssitianceRequesters.CreateMenu(menuAssistanceRequesters);

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

            API.RegisterKeyMapping(COMMAND_OPEN_MENU, "Open Police Interactive Menu", "keyboard", "F1");
            API.RegisterCommand(COMMAND_OPEN_MENU, new Action(OnMenuCommand), false);
        }

        private void MenuMain_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.Opened)
                OnMenuState(true);

            if (state == MenuState.Closed)
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
            BaseScript.TriggerEvent("curiosity:Client:Menu:IsOpened", isOpen);
        }

        [TickHandler]
        private async Task OnShowHelperMessage()
        {
            if (!JobManager.IsOfficer) return; // no point in showing if their're not an officer

            if (MarkerManager.GetActiveMarker(MarkerFilter.Unknown) != null) return;

            if (!Cache.PlayerPed.IsInVehicle())
            {
                CanShowMessage = true;
            }

            if (Cache.PlayerPed.IsInVehicle() && Cache.PlayerPed?.CurrentVehicle?.Speed > 4f)
            {
                if (!HasShownWarning)
                    HelpMessage.Custom($"", 1000, false);

                HasShownWarning = true;
                CanShowMessage = true;

                return;
            }

            List<CitizenFX.Core.Ped> peds = World.GetAllPeds().Where(x => x.IsInRangeOf(Cache.PlayerPed.Position, 2f) && Decorators.GetBoolean(x.Handle, Decorators.PED_MISSION)).Select(p => p).ToList();

            List<CitizenFX.Core.Vehicle> vehicles = World.GetAllVehicles().Where(x => x.IsInRangeOf(Cache.PlayerPed.Position, 4f)
                && (Decorators.GetBoolean(x.Handle, Decorators.VEHICLE_MISSION)
                || (Decorators.GetBoolean(x.Handle, Decorators.PLAYER_VEHICLE) && Decorators.GetInteger(x.Handle, Decorators.PLAYER_OWNER) == Game.Player.ServerId
                    && (PlayerManager.GetModule().PersonalVehicle.ClassType == VehicleClass.Emergency || PlayerManager.GetModule().PersonalVehicle.Model.Hash == (int)VehicleHash.Polmav)))
                ).Select(p => p).ToList();

            int interactables = peds.Count + vehicles.Count; // near any interactives?

            if (interactables == 0)
            {
                await BaseScript.Delay(1000);
                return;
            }

            if (!API.IsHelpMessageBeingDisplayed() && !_MenuPool.IsAnyMenuOpen() && CanShowMessage)
            {
                HelpMessage.CustomLooped(HelpMessage.Label.MENU_OPEN);
                HasShownWarning = false;
                CanShowMessage = false;
            }
        }

        public void OnMenuCommand()
        {
            if (Cache.PlayerPed.IsAlive && JobManager.IsOfficer && !_MenuPool.IsAnyMenuOpen())
            {
                if (menuMain.Visible) return;

                if (!menuMain.Visible)
                {
                    menuMain.Visible = !menuMain.Visible;
                    Instance.AttachTickHandler(OnMenuDisplay);
                }
            }
        }

        public static Ped GetClosestInteractivePed()
        {
            if (!Mission.isOnMission) return null;

            return Mission.RegisteredPeds.Select(x => x).Where(p => p.IsInRangeOf(Cache.PlayerPed.Position, 2f) && p.IsMission).FirstOrDefault();
        }

        public static Vehicle GetClosestVehicle()
        {
            if (!Mission.isOnMission) return null;

            return Mission.RegisteredVehicles.Select(x => x).Where(p => p.IsInRangeOf(Cache.PlayerPed.Position, 4f) && p.IsMission).FirstOrDefault();
        }
    }
}
