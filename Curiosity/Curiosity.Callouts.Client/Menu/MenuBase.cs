using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Callouts.Client.Managers;
using Curiosity.Callouts.Client.Utils;
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

        private bool IsPanicButtonCooldownActive = false;

        private UIMenuItem mItemRequestAssistance = new UIMenuItem($"Request Assistance", "Call for support");
        private UIMenuItem mItemPanicButton = new UIMenuItem($"Panic Button", "Call dispatch for assistance.~n~~o~5 Minute cooldown between requests");

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
            _MenuPool.Add(menuMain);

            menuMain.OnItemSelect += MenuMain_OnItemSelect;
            menuMain.OnMenuOpen += MenuMain_OnMenuOpen;
            menuMain.OnMenuClose += MenuMain_OnMenuClose;

            menuMain.AddItem(mItemPanicButton);
            menuMain.AddItem(mItemRequestAssistance);

            _MenuPool.RefreshIndex();
        }

        private void MenuMain_OnMenuOpen(UIMenu sender)
        {
            OnMenuState(true);

            // Change button states

            bool isPursuitActive = PursuitManager.IsPursuitActive;

            mItemRequestAssistance.Enabled = isPursuitActive;
            mItemRequestAssistance.Description = isPursuitActive ? "Call an NPC for assistance" : "Currently not in an active pursuit";
        }

        async Task OnPanicButtonCooldown()
        {
            long ggt = API.GetGameTimer();
            while ((API.GetGameTimer() - ggt) < 300000) // 5 minutes
            {
                await BaseScript.Delay(100);
            }
            UiTools.Dispatch("Units Available", "Units have now returned to the depo");
            IsPanicButtonCooldownActive = false;
            PluginInstance.DeregisterTickHandler(OnPanicButtonCooldown);
        }

        private async void MenuMain_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == mItemRequestAssistance)
            {
                PursuitManager.AddNewCopToPursuit();
            }

            if (selectedItem == mItemPanicButton)
            {
                if (IsPanicButtonCooldownActive)
                {
                    UiTools.Dispatch("Request Denied", "Requested units are currently responding to another call");
                    return;
                }

                IsPanicButtonCooldownActive = true;
                PluginInstance.RegisterTickHandler(OnPanicButtonCooldown);

                int numCops = Utility.RANDOM.Next(3, 10);

                for (var i = 0; i < numCops; i++)
                {
                    Vehicle copCar = await World.CreateVehicle(Collections.PoliceCars.URBAN.Random(),
                        Game.PlayerPed.Position.AroundStreet(200f, 2000f));
                    copCar.IsSirenActive = true;

                    Ped cop = await World.CreatePed(PedHash.Cop01SMY, copCar.Position + copCar.UpVector * 5f);
                    cop.SetIntoVehicle(copCar, VehicleSeat.Driver);
                    Blip blip = cop.AttachedBlip;

                    if (blip == null)
                    {
                        blip = cop.AttachBlip();
                    }

                    if (blip != null)
                    {
                        blip.Color = BlipColor.Blue;
                        blip.IsFriendly = true;
                    }

                    TaskSequence sequence = new TaskSequence();

                    sequence.AddTask.DriveTo(copCar, Game.PlayerPed.Position, 15f, float.MaxValue, (int)DrivingStyle.Rushed);
                    sequence.AddTask.LeaveVehicle();
                    sequence.AddTask.ChatTo(Game.PlayerPed);
                    sequence.AddTask.WanderAround();
                    sequence.Close();

                    cop.Task.PerformSequence(sequence);

                    copCar.IsPersistent = false;
                }
            }
        }

        private void MenuMain_OnMenuClose(UIMenu sender)
        {
            OnMenuState();
            PluginInstance.DeregisterTickHandler(OnMenuDisplay);
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
            if (Game.PlayerPed.IsAlive && PlayerManager.IsOfficer)
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
