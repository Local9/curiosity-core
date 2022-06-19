using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Utils;
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
        UIMenuListItem uiLstCompanions;

        UIMenuItem uiItemRemoveCompanion = new UIMenuItem("Remove Companions", "This will remove all companions.");
        UIMenuItem uiItemResetCharacter = new UIMenuItem("Reset Character");
        UIMenuItem uiStartAutoDrive = new UIMenuItem("Start Auto Drive", "Vehicle will drive to the waypoint you have set.");

        List<Companion> companions;
        List<SupporterModel> playerModels;

        public async void CreateMenu(UIMenu menu)
        {
            await Session.Loading();

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
                if (Cache.Player.User.IsTrustedAdmin)
                {
                    Companion juggernaut = new Companion();
                    juggernaut.Human = true;
                    juggernaut.Label = "Juggernaut";
                    juggernaut.Hash = "u_m_y_juggernaut_01";
                    companions.Add(juggernaut);
                }

                uiLstCompanions = new UIMenuListItem("Companion", companions.Select(x => x.Label).ToList<dynamic>(), 0);
                baseMenu.AddItem(uiLstCompanions);
                baseMenu.AddItem(uiItemRemoveCompanion);
            }

            baseMenu.AddItem(uiStartAutoDrive);

            baseMenu.OnItemSelect += BaseMenu_OnItemSelect;
            baseMenu.OnListSelect += BaseMenu_OnListSelect;
            baseMenu.OnMenuStateChanged += BaseMenu_OnMenuStateChanged;
        }

        private void BaseMenu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            //uiLstCompanions.Enabled = Cache.Player.User.IsStaff;
            //uiItemRemoveCompanion.Enabled = Cache.Player.User.IsStaff;
            //uiLstCompanions.Description = "Currently Disabled due to online issues";
            //if (uiLstCompanions.Enabled)
            //    uiLstCompanions.Description = "Able to create a companion.";
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
            else if (selectedItem == uiItemRemoveCompanion)
            {
                CompanionManager.GetModule().RemoveCompanions();
            }
            else if (selectedItem == uiStartAutoDrive)
            {
                VehicleManager.GetModule().EnableAutodrive();
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
            else if (listItem == uiLstCompanions)
            {
                Companion companion = companions[newIndex];
                string modelHash = companion.Hash;
                bool isHuman = companion.Human;

                if (!isHuman)
                    NotificationManager.GetModule().Error($"Currently unable to handle animals.");
                // CompanionManager.GetModule().SpawnNonHuman((uint)model);

                if (isHuman)
                {
                    int ped = await CompanionManager.GetModule().SpawnHuman(modelHash);

                    if (ped != -1)
                        AdditionalPedConfiguration(modelHash, ped);
                }

                goto Exit;
            }

        ExitAndFade:
            await Cache.PlayerPed.FadeIn();
        Exit:
            listItem.Enabled = true;
        }

        private static void AdditionalPedConfiguration(string modelHash, int ped)
        {
            if (modelHash == "u_m_y_juggernaut_01" && Cache.Player.User.IsTrustedAdmin)
            {
                Ped companionPed = new Ped(ped);
                companionPed.SetConfigFlag((int)ePedConfigFlags.CPED_CONFIG_FLAG_DieWhenRagdoll, false);
                companionPed.SetConfigFlag((int)ePedConfigFlags.CPED_CONFIG_FLAG_DisableHurt, true);
                companionPed.SetConfigFlag((int)ePedConfigFlags.CPED_CONFIG_FLAG_DisableShockingEvents, true);
                companionPed.SetConfigFlag((int)ePedConfigFlags.CPED_CONFIG_FLAG_IgnoreBeingOnFire, true);
                companionPed.SetConfigFlag((int)ePedConfigFlags.CPED_CONFIG_FLAG_IgnoreSeenMelee, true);
                companionPed.CanSufferCriticalHits = false;

                companionPed.Health = companionPed.MaxHealth;
                companionPed.Armor = 200;
                companionPed.DropsWeaponsOnDeath = false;

                companionPed.Accuracy = Utility.RANDOM.Next(20, 100);

                int type = Utility.RANDOM.Next(3);
                if (type == 0)
                {
                    SetPedPropIndex(companionPed.Handle, 0, 0, 0, false);
                    SetPedComponentVariation(companionPed.Handle, 0, 0, 1, 0);
                    SetPedComponentVariation(companionPed.Handle, 3, 0, 0, 0);
                    SetPedComponentVariation(companionPed.Handle, 4, 0, 0, 0);
                    SetPedComponentVariation(companionPed.Handle, 5, 0, 0, 0);
                    SetPedComponentVariation(companionPed.Handle, 8, 0, 1, 0);
                    SetPedComponentVariation(companionPed.Handle, 10, 0, 1, 0);
                }
                else if (type == 1)
                {
                    SetPedPropIndex(companionPed.Handle, 0, 0, 0, false);
                    SetPedComponentVariation(companionPed.Handle, 0, 0, 0, 0);
                    SetPedComponentVariation(companionPed.Handle, 3, 0, 1, 0);
                    SetPedComponentVariation(companionPed.Handle, 4, 0, 0, 0);
                    SetPedComponentVariation(companionPed.Handle, 5, 0, 0, 0);
                    SetPedComponentVariation(companionPed.Handle, 8, 0, 0, 0);
                    SetPedComponentVariation(companionPed.Handle, 10, 0, 0, 0);
                }
                else
                {
                    ClearPedProp(companionPed.Handle, 0);
                    SetPedComponentVariation(companionPed.Handle, 0, 0, 0, 0);
                    SetPedComponentVariation(companionPed.Handle, 3, 0, 1, 0);
                    SetPedComponentVariation(companionPed.Handle, 4, 0, 0, 0);
                    SetPedComponentVariation(companionPed.Handle, 5, 0, 0, 0);
                    SetPedComponentVariation(companionPed.Handle, 8, 0, 0, 0);
                    SetPedComponentVariation(companionPed.Handle, 10, 0, 0, 0);
                }

                companionPed.Weapons.RemoveAll();
                companionPed.Weapons.Give(WeaponHash.Minigun, 999, false, true);
                companionPed.Health = 5000;
                companionPed.CanRagdoll = false;
                companionPed.IsMeleeProof = true;
                companionPed.FiringPattern = FiringPattern.FullAuto;
                companionPed.MovementAnimationSet = "move_m@bag";
            }
        }
    }
}
