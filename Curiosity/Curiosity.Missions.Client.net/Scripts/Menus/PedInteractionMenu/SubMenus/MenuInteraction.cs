using Curiosity.Missions.Client.net.MissionPeds;
using Curiosity.Shared.Client.net.Extensions;
using MenuAPI;
using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.Missions.Client.net.Scripts.Interactions.PedInteractions;
using Curiosity.Shared.Client.net;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Global.Shared.net;
using Newtonsoft.Json;
using Curiosity.Missions.Client.net.Helpers;

namespace Curiosity.Missions.Client.net.Scripts.Menus.PedInteractionMenu.SubMenus
{
    class MenuInteraction
    {
        private const string MENU_TITLE = "Interactions";
        static Menu menu;
        static InteractivePed _interactivePed;

        static MenuItem mItemHandcuffs = new MenuItem("Apply Handcuffs");
        static MenuItem mItemDetainInCurrentVehicle = new MenuItem("Detain in Vehicle");
        // tests
        static MenuItem mItemSuspectVehicle = new MenuItem("Suspect: Ask to Leave Vehicle");
        static MenuItem mItemBreathalyzer = new MenuItem("Suspect: Breathalyzer");
        static MenuItem mItemSearch = new MenuItem("Suspect: Search");
        static MenuItem mItemDrugTest = new MenuItem("Suspect: Drug test");

        static MenuItem mItemGrabPed = new MenuItem("Suspect: Grab");

        static MenuItem mItemWarn = new MenuItem("Suspect: Warn");
        static MenuItem mItemRelease = new MenuItem("Suspect: Release");

        // Speeding Interaction
        static MenuItem mItemSpeedingTicket = new MenuItem("Suspect: Speeding Ticket");

        // Dead Interactions
        static MenuItem mItemCallCoroner = new MenuItem("Call Coroner");
        static MenuItem mItemCpr = new MenuItem("CPR");

        static public void SetupMenu(InteractivePed interactivePed)
        {
            if (menu == null)
            {
                menu = new Menu(MENU_TITLE, MENU_TITLE);
                menu.OnMenuClose += Menu_OnMenuClose;
                menu.OnMenuOpen += Menu_OnMenuOpen;
                menu.OnItemSelect += Menu_OnItemSelect;
                menu.EnableInstructionalButtons = true;
            }
            menu.MenuTitle = MENU_TITLE;
            _interactivePed = interactivePed;
            MenuBase.AddSubMenu(MenuBase.MainMenu, menu);
        }

        private async static void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            // DEAD
            if (menuItem == mItemCpr)
            {
                Cpr.Init();
                await Client.Delay(1000);
                Cpr.InteractionCPR(_interactivePed);
                MenuController.CloseAllMenus();
                Client.TriggerEvent("curiosity:interaction:closeMenu");
                return;
            }
            if (menuItem == mItemSpeedingTicket)
            {
                TrafficStopData trafficStopData = new TrafficStopData();
                trafficStopData.Ticket = true;

                string jsonString = Encode.StringToBase64(JsonConvert.SerializeObject(trafficStopData));
                Client.TriggerServerEvent("curiosity:Server:Missions:TrafficStop", jsonString);

                Animations.AnimationClipboard();

                MenuController.CloseAllMenus();
                Client.TriggerEvent("curiosity:interaction:closeMenu");
                await BaseScript.Delay(0);
                Client.TriggerEvent("curiosity:interaction:released", _interactivePed.Handle);
                await BaseScript.Delay(0);
                Client.TriggerEvent("curiosity:setting:group:leave", _interactivePed.Handle, Game.PlayerPed.PedGroup.Handle);
                return;
            }
            if (menuItem == mItemCallCoroner)
            {
                Extras.Coroner.RequestService();
                MenuController.CloseAllMenus();
                Client.TriggerEvent("curiosity:interaction:closeMenu");
                return;
            }
            // ALIVE
            if (menuItem == mItemHandcuffs)
            {
                ArrestInteractions.InteractionHandcuff(_interactivePed);
                mItemHandcuffs.Enabled = false;
                await Client.Delay(4000);
                mItemHandcuffs.Text = _interactivePed.IsHandcuffed ? "Remove Handcuffs" : "Apply Handcuffs";
                mItemHandcuffs.Enabled = true;
                mItemDetainInCurrentVehicle.Enabled = CanDetainPed();
                return;
            }
            if (menuItem == mItemDetainInCurrentVehicle)
            {
                ArrestInteractions.InteractionPutInVehicle(_interactivePed);
                mItemDetainInCurrentVehicle.Enabled = false;
                await Client.Delay(4000);
                mItemDetainInCurrentVehicle.Enabled = CanDetainPed();
                mItemDetainInCurrentVehicle.Text = _interactivePed.Ped.IsInVehicle() && _interactivePed.Ped.CurrentVehicle == Client.CurrentVehicle ? "Remove from Vehicle" : "Detain in Vehicle";
                return;
            }

            // Main Interactions
            if (menuItem == mItemBreathalyzer)
            {
                Generic.InteractionBreathalyzer(_interactivePed);
                return;
            }

            if (menuItem == mItemDrugTest)
            {
                Generic.InteractionDrugTest(_interactivePed);
                return;
            }

            if (menuItem == mItemSearch)
            {
                Generic.InteractionSearch(_interactivePed);
                return;
            }

            if (menuItem == mItemGrabPed)
            {
                if (_interactivePed.Position.Distance(Game.PlayerPed.Position) > 2)
                {
                    CitizenFX.Core.UI.Screen.ShowNotification("~r~Must be closer to grab the ped");
                    return;
                }
                Client.TriggerEvent("curiosity:interaction:grab", _interactivePed.Handle);

                //MenuController.CloseAllMenus();
                //Client.TriggerEvent("curiosity:interaction:closeMenu");
                return;
            }

            if (menuItem == mItemWarn)
            {
                ArrestInteractions.InteractionIssueWarning(_interactivePed);
                MenuController.CloseAllMenus();
                Client.TriggerEvent("curiosity:interaction:closeMenu");
                await BaseScript.Delay(0);
                Client.TriggerEvent("curiosity:interaction:released", _interactivePed.Handle);
                await BaseScript.Delay(0);
                Client.TriggerEvent("curiosity:setting:group:leave", _interactivePed.Handle, Game.PlayerPed.PedGroup.Handle);
                return;
            }

            if (menuItem == mItemRelease)
            {
                ArrestInteractions.InteractionRelease(_interactivePed);
                MenuController.CloseAllMenus();
                Client.TriggerEvent("curiosity:interaction:closeMenu");
                await BaseScript.Delay(0);
                Client.TriggerEvent("curiosity:interaction:released", _interactivePed.Handle);
                await BaseScript.Delay(0);
                Client.TriggerEvent("curiosity:setting:group:leave", _interactivePed.Handle, Game.PlayerPed.PedGroup.Handle);
                return;
            }

            if (menuItem == mItemSuspectVehicle && _interactivePed.Ped.IsInVehicle())
            {
                Generic.InteractionLeaveVehicle(_interactivePed);
                
                if (_interactivePed.Ped.CurrentVehicle != null)
                    DecorSetInt(_interactivePed.Ped.Handle, Client.DECOR_NPC_CURRENT_VEHICLE, _interactivePed.Ped.CurrentVehicle.Handle);

                mItemSuspectVehicle.Enabled = false;
                await Client.Delay(2000);
                mItemSuspectVehicle.Text = "Suspect: Return to Vehicle";
                mItemSuspectVehicle.Enabled = true;
                mItemHandcuffs.Enabled = !_interactivePed.Ped.IsInVehicle();
                return;
            }

            if (menuItem == mItemSuspectVehicle && !_interactivePed.Ped.IsInVehicle() && DecorExistOn(_interactivePed.Ped.Handle, Client.DECOR_NPC_CURRENT_VEHICLE))
            {
                Generic.InteractionEnterVehicle(_interactivePed);
                
                mItemSuspectVehicle.Enabled = false;
                await Client.Delay(2000);
                mItemSuspectVehicle.Text = "Suspect: Ask to Leave Vehicle";
                mItemSuspectVehicle.Enabled = true;
                mItemHandcuffs.Enabled = _interactivePed.Ped.IsInVehicle();
                return;
            }
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            Log.Info($"{MENU_TITLE} Open");

            menu.MenuTitle = "";
            menu.MenuSubtitle = MENU_TITLE;

            MenuBase.MenuState(true);
            menu.ClearMenuItems();

            bool IsInVehicle = _interactivePed.Ped.IsInVehicle();

            if (_interactivePed.IsDead)
            {
                // CPR / Coroner
                if (!_interactivePed.HasCprFailed)
                    menu.AddMenuItem(mItemCpr);

                menu.AddMenuItem(mItemCallCoroner);
            }
            else
            {
                mItemSuspectVehicle.Text = "Suspect: Ask to Leave Vehicle";
                if (!_interactivePed.Ped.IsInVehicle() && DecorExistOn(_interactivePed.Ped.Handle, Client.DECOR_NPC_CURRENT_VEHICLE))
                {
                    mItemSuspectVehicle.Text = "Suspect: Return to Vehicle";
                }
                menu.AddMenuItem(mItemSuspectVehicle);


                mItemHandcuffs.Text = _interactivePed.IsHandcuffed ? "Remove Handcuffs" : "Apply Handcuffs";
                mItemHandcuffs.Enabled = !IsInVehicle;
                menu.AddMenuItem(mItemHandcuffs);

                mItemDetainInCurrentVehicle.Enabled = CanDetainPed();
                mItemDetainInCurrentVehicle.Text = IsInVehicle ? "Remove from Vehicle" : "Detain in Vehicle";
                menu.AddMenuItem(mItemDetainInCurrentVehicle);

                menu.AddMenuItem(mItemBreathalyzer);
                menu.AddMenuItem(mItemDrugTest);

                mItemSearch.Enabled = !IsInVehicle;
                mItemSearch.Description = IsInVehicle ? "Suspect must be removed from the vehicle before searching." : string.Empty;
                menu.AddMenuItem(mItemSearch);

                if (_interactivePed.GetBoolean(Client.DECOR_VEHICLE_SPEEDING))
                {
                    menu.AddMenuItem(mItemSpeedingTicket);
                }

                mItemGrabPed.Text = _interactivePed.HasBeenGrabbed ? "Suspect: Stop Holding" : "Suspect: Grab";
                menu.AddMenuItem(mItemGrabPed);

                menu.AddMenuItem(mItemWarn);

                menu.AddMenuItem(mItemRelease);
            }
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            MenuController.MainMenu.OpenMenu();
        }

        private static bool CanDetainPed()
        {
            return _interactivePed.IsHandcuffed && Client.CurrentVehicle != null;
        }
    }
}
