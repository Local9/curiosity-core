using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Managers;
using Curiosity.Core.Client.Managers.UI;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.Shop;
using NativeUI;
using System.Collections.Generic;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu.Inventory
{
    class HealthMenu
    {
        UIMenu baseMenu;

        UIMenuItem miLightKit = new UIMenuItem("Light Kit", "Use a Light Kit to refill your health bar.");
        UIMenuItem miMediumKit = new UIMenuItem("Meduim Kit", "Use a Meduim Kit to refill your health bar.");
        UIMenuItem miLargeKit = new UIMenuItem("Large Kit", "Use a Large Kit to refill your health bar.");

        EventSystem EventSystem => EventSystem.GetModule();

        public void CreateMenu(UIMenu menu)
        {
            baseMenu = menu;

            miLightKit.SetRightLabel($"0");
            miMediumKit.SetRightLabel($"0");
            miLargeKit.SetRightLabel($"0");

            baseMenu.AddItem(miLightKit);
            baseMenu.AddItem(miMediumKit);
            baseMenu.AddItem(miLargeKit);

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

            ExportMessage result = await InventoryManager.GetModule().UseItem(data.id);

            if (result.Success)
            {
                selectedItem.SetRightLabel($"{data.amt - 1}");
                selectedItem.Enabled = true;
                return;
            }

            selectedItem.Enabled = true;
            NotificationManager.GetModule().Error(result.Error);
        }

        private void BaseMenu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state.Equals(MenuState.Opened) || state.Equals(MenuState.ChangeForward))
                UpdateMenuItems();
        }

        private async void UpdateMenuItems()
        {
            // GET ARMOR COUNTS FROM SERVER
            List<CharacterKit> kits = await EventSystem.Request<List<CharacterKit>>("character:inventory:health");

            foreach(CharacterKit kit in kits)
            {
                switch (kit.ItemId)
                {
                    case 424:
                        miLightKit.SetRightLabel($"{kit.NumberOwned}");
                        miLightKit.ItemData = new { amt = kit.NumberOwned, id = kit.ItemId };
                        miLightKit.Enabled = kit.NumberOwned > 0;
                        break;
                    case 425:
                        miMediumKit.SetRightLabel($"{kit.NumberOwned}");
                        miMediumKit.ItemData = new { amt = kit.NumberOwned, id = kit.ItemId };
                        miMediumKit.Enabled = kit.NumberOwned > 0;
                        break;
                    case 426:
                        miLargeKit.SetRightLabel($"{kit.NumberOwned}");
                        miLargeKit.ItemData = new { amt = kit.NumberOwned, id = kit.ItemId };
                        miLargeKit.Enabled = kit.NumberOwned > 0;
                        break;
                }
            }
        }
    }
}
