using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.Entity;
using Curiosity.Police.Client.net.Extensions;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Shared.Client.net.Helper;
using MenuAPI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Police.Client.net.Classes.Menus
{
    class PoliceDispatchMenu
    {
        static Client client = Client.GetInstance();
        static Menu calloutMenu;
        static bool SpikeStripDeployed = false;
        // ITEMS
        static private MenuItem menuItemSpikeStrip = new MenuItem("Deploy Spike Strip");

        static private MenuItem rbItemCode2 = new MenuItem("Code 2: Urgent - Proceed immediately");
        static private MenuItem rbItemCode3 = new MenuItem("Code 3: Emergency");
        static private MenuItem rbItemCode4 = new MenuItem("Code 4: No further assistance required");
        // SETTINGS
        static bool HasRequestedBackup = false;
        static bool IsMenuOpen = false;

        static public void Init()
        {
            client.RegisterTickHandler(OnTaskHasRequestedBackup);
        }

        static public async Task OnTaskKeyCombination()
        {
            if (ControlHelper.IsControlJustPressed(Control.Pickup, true, ControlModifier.Alt))
            {
                if (!IsMenuOpen)
                    OpenMenu();
            }
            await Task.FromResult(0);
        }

        static async Task OnTaskHasRequestedBackup()
        {
            long gameTimer = GetGameTimer();
            while (HasRequestedBackup)
            {
                if ((GetGameTimer() - gameTimer) > 30000)
                {
                    HasRequestedBackup = false;
                    client.DeregisterTickHandler(OnTaskHasRequestedBackup);
                }
                await Client.Delay(1000);
            }
        }

        static async Task OnMenuOpenControls()
        {
            await Task.FromResult(0);

            DisableControlAction(0, 86, true);
            DisableControlAction(0, (int)Control.VehicleCinCam, true);
            DisableControlAction(0, (int)Control.VehicleLookBehind, true);
            DisableControlAction(0, (int)Control.LookBehind, true);
        }

        static public void OpenMenu()
        {
            IsMenuOpen = true;

            MenuBaseFunctions.MenuOpen();

            MenuController.DontOpenAnyMenu = false;
            MenuController.EnableMenuToggleKeyOnController = false;

            if (calloutMenu == null)
            {
                calloutMenu = new Menu("Dispatch", "Dispatch Options");

                calloutMenu.OnMenuOpen += OnMenuOpen;
                calloutMenu.OnItemSelect += OnItemSelect;
                calloutMenu.OnMenuClose += OnMenuClose;

                MenuController.AddMenu(calloutMenu);

                MenuController.EnableMenuToggleKeyOnController = false;
            }

            calloutMenu.OpenMenu();
        }

        private static void OnMenuOpen(Menu menu)
        {
            client.RegisterTickHandler(OnMenuOpenControls);

            MenuBaseFunctions.MenuOpen();
            MenuController.DontOpenAnyMenu = false;
            MenuController.EnableMenuToggleKeyOnController = false;

            Skills skill = Player.PlayerInformation.playerInfo.Skills["policexp"];

            if (skill != null)
            {

                long policexp = skill.Value;
                if (policexp >= 5000)
                {
                    menuItemSpikeStrip.Text = SpikeStripDeployed ? "Remove Spike Strip" : "Deploy Spike Strip";
                    menu.AddMenuItem(menuItemSpikeStrip);
                }
            }

            menu.AddMenuItem(rbItemCode2);
            menu.AddMenuItem(rbItemCode3);
            menu.AddMenuItem(rbItemCode4);
        }

        private static void OnMenuClose(Menu menu)
        {
            IsMenuOpen = false;
            calloutMenu.ClearMenuItems();
            MenuController.DontOpenAnyMenu = true;
            MenuBaseFunctions.MenuClose();

            client.DeregisterTickHandler(OnMenuOpenControls);
        }

        private static void OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            Vector3 pos = Game.PlayerPed.Position;

            if (menuItem == menuItemSpikeStrip)
            {
                if (SpikeStripDeployed)
                {
                    RemoveSpikeStrip();
                    return;
                }
                DeploySpikeStrip();
                return;
            }

            if (HasRequestedBackup)
            {
                if (menuItem == rbItemCode4)
                {
                    BaseScript.TriggerServerEvent("curiosity:Server:Player:Backup", 4, pos.X, pos.Y, pos.Z);
                    HasRequestedBackup = false;
                    client.DeregisterTickHandler(OnTaskHasRequestedBackup);
                    MenuController.CloseAllMenus();
                }
                else
                {
                    Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Dispatch", $"Please wait...", "Assistance has been requested.", 2);
                }
                return;
            }

            if (menuItem == rbItemCode2)
            {
                BaseScript.TriggerServerEvent("curiosity:Server:Player:Backup", 2, pos.X, pos.Y, pos.Z);
            }

            if (menuItem == rbItemCode3)
            {
                BaseScript.TriggerServerEvent("curiosity:Server:Player:Backup", 3, pos.X, pos.Y, pos.Z);
            }

            if (menuItem == rbItemCode4)
            {
                BaseScript.TriggerServerEvent("curiosity:Server:Player:Backup", 4, pos.X, pos.Y, pos.Z);
            }

            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Dispatch", $"......", "Assistance has been requested.", 2);

            HasRequestedBackup = true;
            client.RegisterTickHandler(OnTaskHasRequestedBackup);
            MenuController.CloseAllMenus();
        }

        static string stingerHash = "P_ld_stinger_s";
        static Vector3 stingerOffset;

        private async static void DeploySpikeStrip()
        {
            if (MenuPoliceOptions.GetVehicleDriving(Game.PlayerPed) != null)
            {
                int model = API.GetHashKey(stingerHash);
                stingerOffset = API.GetOffsetFromEntityInWorldCoords(Game.PlayerPed.CurrentVehicle.Handle, 0.0f, -5.2f, -0.25f);

                API.RequestScriptAudioBank("BIG_SCORE_HIJACK_01", true);
                await Client.Delay(500);

                API.RequestModel((uint)model);
                while (!API.HasModelLoaded((uint)model))
                {
                    await Client.Delay(0);
                }

                if (API.HasModelLoaded((uint)model))
                {
                    int spikeObject = API.CreateObject(model, stingerOffset.X, stingerOffset.Y, stingerOffset.Z, true, false, true);
                    API.SetEntityNoCollisionEntity(spikeObject, Game.PlayerPed.Handle, true);
                    API.SetEntityDynamic(spikeObject, false);
                    API.ActivatePhysics(spikeObject);

                    if (API.DoesEntityExist(spikeObject))
                    {
                        float height = API.GetEntityHeightAboveGround(spikeObject);
                        API.SetEntityCoords(spikeObject, stingerOffset.X, stingerOffset.Y, stingerOffset.Z - height + 0.05f, false, false, false, false);
                        API.SetEntityHeading(spikeObject, Game.PlayerPed.Heading - 80.0f);
                        API.SetEntityCollision(spikeObject, false, false);
                        API.PlaceObjectOnGroundProperly(spikeObject);
                        API.SetEntityAsMissionEntity(spikeObject, false, false);
                        API.SetModelAsNoLongerNeeded((uint)model);
                        API.PlaySoundFromEntity(-1, "DROP_STINGER", Game.PlayerPed.Handle, "BIG_SCORE_3A_SOUNDS", false, 0);
                    }
                    SpikeStripDeployed = true;
                    Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Dispatch", $"......", "Spike Strip Deployed.", 2);
                    client.RegisterTickHandler(OnVehicleNearSpikeStrip);
                }
            }
            else
            {
                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Dispatch", $"......", "You must be inside a valid vehicle.", 2);
            }
        }

        private static void RemoveSpikeStrip()
        {
            Vector3 position = Game.PlayerPed.Position;
            int model = API.GetHashKey(stingerHash);
            if (API.DoesObjectOfTypeExistAtCoords(position.X, position.Y, position.Z, 0.9f, (uint)model, true))
            {
                int spike = API.GetClosestObjectOfType(position.X, position.Y, position.Z, 0.9f, (uint)model, false, false, false);
                API.DeleteObject(ref spike);
                SpikeStripDeployed = false;
                client.DeregisterTickHandler(OnVehicleNearSpikeStrip);
            }
        }

        static async Task OnVehicleNearSpikeStrip()
        {
            if (!SpikeStripDeployed) return;

            List<Vehicle> vehicles = World.GetAllVehicles().Select(v => v).Where(p => p.Position.Distance(stingerOffset, false) <= 1f && p.Driver != null && !p.Driver.IsPlayer).ToList();

            if (vehicles == null) return;

            foreach (Vehicle vehicle in vehicles)
            {
                if (vehicle.CanTiresBurst && !AreTiresBurst(vehicle) && Decorators.GetBoolean(vehicle.Handle, "curiosity::police::vehicle::mission"))
                {
                    API.SetVehicleTyreBurst(vehicle.Handle, 0, true, 1000.0f);
                }
            }

            await Client.Delay(10);
        }

        static bool AreTiresBurst(Vehicle vehicle)
        {
            return API.IsVehicleTyreBurst(vehicle.Handle, 0, true) && API.IsVehicleTyreBurst(vehicle.Handle, 1, true) && API.IsVehicleTyreBurst(vehicle.Handle, 4, true) && API.IsVehicleTyreBurst(vehicle.Handle, 5, true);
        }
    }
}
