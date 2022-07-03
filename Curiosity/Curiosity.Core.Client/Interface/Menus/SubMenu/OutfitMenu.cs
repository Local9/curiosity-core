using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Models;
using NativeUI;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class OutfitMenu
    {
        private UIMenu menu;
        UIMenuListItem uiListOutfits;
        UIMenuItem uiItemResetCharacter = new UIMenuItem("Reset Character");
        ConfigurationManager configuration = ConfigurationManager.GetModule();

        public async Task CreateMenuAsync(UIMenu uiMenu)
        {
            await Session.Loading();
            menu = uiMenu;

            List<dynamic> dynList = new();

            foreach(Outfit outfit in configuration.Outfits())
            {
                if (Cache.Character.IsMale == outfit.IsMale)
                    dynList.Add(outfit);
            }

            uiListOutfits = new UIMenuListItem("Job Outfits", dynList, 0);
            uiListOutfits.Description = "Press ENTER to select the outfit of your choice.";
            menu.AddItem(uiListOutfits);

            menu.AddItem(uiItemResetCharacter);

            menu.OnItemSelect += Menu_OnItemSelectAsync;
            menu.OnListSelect += Menu_OnListSelect;
        }

        private async void Menu_OnListSelect(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (listItem == uiListOutfits)
            {
                listItem.Enabled = false;
                if (Cache.PlayerPed.IsDead)
                {
                    NotificationManager.GetModule().Error($"Computer said no.");
                    listItem.Enabled = true;
                    return;
                }

                await Cache.PlayerPed.FadeOut();

                Outfit outfit = (Outfit)listItem.Items[newIndex];
                foreach(OutfitComponent component in outfit.Components)
                {
                    SetPedComponentVariation(PlayerPedId(), component.Component, component.Drawable, component.Texture, 0);
                }

                foreach (OutfitProp prop in outfit.Props)
                {
                    SetPedPropIndex(Cache.PlayerPed.Handle, prop.Index, -1, -1, false);
                    ClearPedProp(Cache.PlayerPed.Handle, prop.Index);

                    SetPedPropIndex(PlayerPedId(), prop.Index, prop.Drawable, prop.Texture, true);
                }

                await BaseScript.Delay(500); // JIC
                listItem.Enabled = true;
                await Cache.PlayerPed.FadeIn();
            }
        }

        private async void Menu_OnItemSelectAsync(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == uiItemResetCharacter)
            {
                if (Cache.PlayerPed.IsDead)
                {
                    NotificationManager.GetModule().Error($"Computer said no.");
                    selectedItem.Enabled = true;
                    return;
                }

                await Cache.PlayerPed.FadeOut();
                Cache.Player.Character.Load();
                await BaseScript.Delay(500); // JIC
                await Cache.PlayerPed.FadeIn();
            }
        }
    }
}
