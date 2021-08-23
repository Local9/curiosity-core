using CitizenFX.Core;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Models;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        UIMenuListItem uiLstCompanions;

        UIMenuItem uiItemRemoveCompanion = new UIMenuItem("Remove Companion");
        UIMenuItem uiItemResetCharacter = new UIMenuItem("Reset Character");

        List<Companion> companions;
        List<SupporterModel> playerModels;

        public void CreateMenu(UIMenu menu)
        {
            baseMenu = menu;
            ConfigurationManager configuration = ConfigurationManager.GetModule();

            companions = configuration.SupporterCompanions();
            playerModels = configuration.SupporterModels();

            if (playerModels is not null)
            {
                uiLstPlayerModels = new UIMenuListItem("Model", playerModels.Select(x => x.Label).ToList<dynamic>(), 0);
                baseMenu.AddItem(uiLstPlayerModels);
                baseMenu.AddItem(uiItemResetCharacter);
            }

            if (companions is not null)
            {
                uiLstCompanions = new UIMenuListItem("Companion", companions.Select(x => x.Label).ToList<dynamic>(), 0);
                baseMenu.AddItem(uiLstCompanions);
                baseMenu.AddItem(uiItemRemoveCompanion);
            }

            baseMenu.OnItemSelect += BaseMenu_OnItemSelect;
            baseMenu.OnListSelect += BaseMenu_OnListSelect;
        }

        private async void BaseMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            selectedItem.Enabled = false;

            if (selectedItem == uiItemResetCharacter)
            {
                await Cache.PlayerPed.FadeOut();
                Cache.Player.Character.Load();
                await BaseScript.Delay(500); // JIC
                await Cache.PlayerPed.FadeIn();
                selectedItem.Enabled = true;
            }
        }

        private async void BaseMenu_OnListSelect(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            listItem.Enabled = false;

            if (listItem == uiLstPlayerModels)
            {
                await Cache.PlayerPed.FadeOut();

                string playerHash = companions[newIndex].Hash;

                Model model = new Model(playerHash);
                
                if (!model.IsInCdImage)
                {
                    NotificationManager.GetModule().Error($"Model is not loaded.");
                    return;
                }

                while (!model.IsLoaded)
                {
                    await model.Request(1000);
                    await BaseScript.Delay(0); // JIC
                }

                Game.Player.ChangeModel(model);

                await Cache.PlayerPed.FadeIn();

                listItem.Enabled = true;
                return;
            }

            if (listItem == uiLstCompanions)
            {

            }
        }
    }
}
