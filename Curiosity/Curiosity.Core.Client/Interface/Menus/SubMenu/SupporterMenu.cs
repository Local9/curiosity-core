using CitizenFX.Core;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Models;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class SupporterMenu
    {
        UIMenu baseMenu;

        /*
         * Supporter Menu
         * Companions
         * Models
         * */

        UIMenuListItem uiLstPlayerModels;

        UIMenuItem uiItemResetCharacter = new UIMenuItem("Reset Character");

        List<SupporterModel> playerModels;

        public void CreateMenu(UIMenu menu)
        {
            baseMenu = menu;
            ConfigurationManager configuration = ConfigurationManager.GetModule();

            playerModels = configuration.SupporterModels();

            if (playerModels is not null)
            {
                uiLstPlayerModels = new UIMenuListItem("Model", playerModels.Select(x => x.Label).ToList<dynamic>(), 0);
                baseMenu.AddItem(uiLstPlayerModels);
                baseMenu.AddItem(uiItemResetCharacter);
            }

            baseMenu.OnItemSelect += BaseMenu_OnItemSelect;
            baseMenu.OnListSelect += BaseMenu_OnListSelect;
        }

        private async void BaseMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            selectedItem.Enabled = false;

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

            selectedItem.Enabled = true;
        }

        private async void BaseMenu_OnListSelect(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            listItem.Enabled = false;

            if (listItem == uiLstPlayerModels)
            {
                await Cache.PlayerPed.FadeOut();

                string modelHash = playerModels[newIndex].Hash;

                Model model = new Model(modelHash);
                
                if (!model.IsInCdImage)
                {
                    NotificationManager.GetModule().Error($"Model is not loaded.");
                    goto ExitAndFade;
                }

                DateTime stopDate = DateTime.UtcNow.AddSeconds(5);

                while (!model.IsLoaded)
                {
                    await model.Request(1000);
                    await BaseScript.Delay(0); // JIC

                    if (DateTime.UtcNow > stopDate) break;
                }

                if (!model.IsLoaded)
                {
                    NotificationManager.GetModule().Error($"Model failed to load.");
                    goto ExitAndFade;
                }

                if (!model.IsValid)
                {
                    NotificationManager.GetModule().Error($"Model is invalid.");
                    goto ExitAndFade;
                }

                Game.Player.ChangeModel(model);
            }

        ExitAndFade:
            await Cache.PlayerPed.FadeIn();
            listItem.Enabled = true;
        }
    }
}
