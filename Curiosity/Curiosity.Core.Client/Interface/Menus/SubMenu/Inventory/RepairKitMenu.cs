using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Managers;
using Curiosity.Core.Client.Managers.UI;
using Curiosity.Core.Client.State;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.Shop;
using NativeUI;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu.Inventory
{
    class RepairKitMenu
    {
        UIMenu baseMenu;

        UIMenuItem miVehicle = new UIMenuItem("Vehicle", "Use a Vehicle repair kit.");
        UIMenuItem miTrailer = new UIMenuItem("Trailer", "Use a Trailer repair kit.");
        UIMenuItem miPlane = new UIMenuItem("Plane", "Use a Plane repair kit.");
        UIMenuItem miBoat = new UIMenuItem("Boat", "Use a Boat repair kit.");
        UIMenuItem miHelicopter = new UIMenuItem("Helicopter", "Use a Helicopter repair kit.");

        public EventSystem EventSystem => EventSystem.GetModule();

        public void CreateMenu(UIMenu menu)
        {
            baseMenu = menu;

            miVehicle.SetRightLabel($"0");
            miTrailer.SetRightLabel($"0");
            miPlane.SetRightLabel($"0");
            miBoat.SetRightLabel($"0");
            miHelicopter.SetRightLabel($"0");

            baseMenu.AddItem(miVehicle);
            baseMenu.AddItem(miTrailer);
            baseMenu.AddItem(miPlane);
            baseMenu.AddItem(miBoat);
            baseMenu.AddItem(miHelicopter);

            baseMenu.OnMenuStateChanged += BaseMenu_OnMenuStateChanged;
            baseMenu.OnItemSelect += BaseMenu_OnItemSelect;
        }

        private async void BaseMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            selectedItem.Enabled = false;
            var data = selectedItem.ItemData;
            if (data.amt == 0)
            {
                NotificationManager.GetModule().Error($"Sorry, you seem to of ran out.");
                selectedItem.Enabled = true;
                return;
            }

            if (data.vehicle == null)
            {
                NotificationManager.GetModule().Error($"Sorry, cannot repair something that doesn't exist.");
                selectedItem.Enabled = true;
                return;
            }

            VehicleState vehicleState = data.vehicle;

            if (!vehicleState.Vehicle.Exists())
            {
                NotificationManager.GetModule().Error($"Sorry, cannot repair something that doesn't exist.");
                selectedItem.Enabled = true;
                return;
            }

            ExportMessage result = await InventoryManager.GetModule().UseItem(data.id, vehicleState.Vehicle);

            if (result.success)
            {
                selectedItem.SetRightLabel($"{data.amt - 1}");
                selectedItem.Enabled = true;
                return;
            }

            selectedItem.Enabled = true;
            NotificationManager.GetModule().Error(result.error);
        }

        private void BaseMenu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state.Equals(MenuState.Opened) || state.Equals(MenuState.ChangeForward))
                UpdateMenuItems();
        }

        private async void UpdateMenuItems()
        {
            // GET ARMOR COUNTS FROM SERVER
            List<CharacterKit> kits = await EventSystem.Request<List<CharacterKit>>("character:inventory:repair");

            foreach (CharacterKit kit in kits)
            {
                switch (kit.ItemId)
                {
                    case 447:
                        miVehicle.SetRightLabel($"{kit.NumberOwned}");
                        miVehicle.ItemData = new { amt = kit.NumberOwned, id = kit.ItemId, vehicle = Cache.PersonalVehicle };
                        miVehicle.Enabled = kit.NumberOwned > 0;
                        break;
                    case 462:
                        miTrailer.SetRightLabel($"{kit.NumberOwned}");
                        miTrailer.ItemData = new { amt = kit.NumberOwned, id = kit.ItemId, vehicle = Cache.PersonalTrailer };
                        miTrailer.Enabled = kit.NumberOwned > 0;
                        break;
                    case 464:
                        miPlane.SetRightLabel($"{kit.NumberOwned}");
                        miPlane.ItemData = new { amt = kit.NumberOwned, id = kit.ItemId, vehicle = Cache.PersonalPlane };
                        miPlane.Enabled = kit.NumberOwned > 0;
                        break;
                    case 463:
                        miBoat.SetRightLabel($"{kit.NumberOwned}");
                        miBoat.ItemData = new { amt = kit.NumberOwned, id = kit.ItemId, vehicle = Cache.PersonalBoat };
                        miBoat.Enabled = kit.NumberOwned > 0;
                        break;
                    case 465:
                        miHelicopter.SetRightLabel($"{kit.NumberOwned}");
                        miHelicopter.ItemData = new { amt = kit.NumberOwned, id = kit.ItemId, vehicle = Cache.PersonalHelicopter };
                        miHelicopter.Enabled = kit.NumberOwned > 0;
                        break;
                }
            }
        }
    }
}
