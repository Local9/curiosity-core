using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Managers;
using Curiosity.Core.Client.Managers.UI;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.Shop;
using NativeUI;
using System.Collections.Generic;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu.Inventory
{
    class BodyArmorMenu
    {
        UIMenu baseMenu;

        UIMenuItem miSuperLightArmor = new UIMenuItem("Super Light Armor", "Use a Super Light Armor to refill your armor bar.");
        UIMenuItem miLightArmor = new UIMenuItem("Light Armor", "Use a Light Armor to refill your armor bar.");
        UIMenuItem miStandardArmor = new UIMenuItem("Standard Armor", "Use a Standard Armor to refill your armor bar.");
        UIMenuItem miHeavyArmor = new UIMenuItem("Heavy Armor", "Use a Heavy Armor to refill your armor bar.");
        UIMenuItem miSuperHeavyArmor = new UIMenuItem("Super Heavy Armor", "Use a Super Heavy Armor to refill your armor bar.");

        public EventSystem EventSystem => EventSystem.GetModule();

        public void CreateMenu(UIMenu menu)
        {
            baseMenu = menu;

            miSuperLightArmor.SetRightLabel($"0");
            miLightArmor.SetRightLabel($"0");
            miStandardArmor.SetRightLabel($"0");
            miHeavyArmor.SetRightLabel($"0");
            miSuperHeavyArmor.SetRightLabel($"0");

            baseMenu.AddItem(miSuperLightArmor);
            baseMenu.AddItem(miLightArmor);
            baseMenu.AddItem(miStandardArmor);
            baseMenu.AddItem(miHeavyArmor);
            baseMenu.AddItem(miSuperHeavyArmor);

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
            List<CharacterKit> kits = await EventSystem.Request<List<CharacterKit>>("character:inventory:armor");

            foreach(CharacterKit kit in kits)
            {
                switch (kit.ItemId)
                {
                    case 448:
                        miSuperLightArmor.SetRightLabel($"{kit.NumberOwned}");
                        miSuperLightArmor.ItemData = new { amt = kit.NumberOwned, id = kit.ItemId };
                        break;
                    case 449:
                        miLightArmor.SetRightLabel($"{kit.NumberOwned}");
                        miLightArmor.ItemData = new { amt = kit.NumberOwned, id = kit.ItemId };
                        break;
                    case 450:
                        miStandardArmor.SetRightLabel($"{kit.NumberOwned}");
                        miStandardArmor.ItemData = new { amt = kit.NumberOwned, id = kit.ItemId };
                        break;
                    case 451:
                        miHeavyArmor.SetRightLabel($"{kit.NumberOwned}");
                        miHeavyArmor.ItemData = new { amt = kit.NumberOwned, id = kit.ItemId };
                        break;
                    case 452:
                        miSuperHeavyArmor.SetRightLabel($"{kit.NumberOwned}");
                        miSuperHeavyArmor.ItemData = new { amt = kit.NumberOwned, id = kit.ItemId };
                        break;
                }
            }
        }
    }
}
