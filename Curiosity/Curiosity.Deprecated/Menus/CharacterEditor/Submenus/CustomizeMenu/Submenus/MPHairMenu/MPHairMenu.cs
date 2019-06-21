﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Client.net.Classes.Menus.CharacterEditor.MPHairMenu;
using Curiosity.Shared.Client.net;

namespace Curiosity.Client.net.Classes.Menus.CharacterEditor.CustomizeMenu
{
    class MPHairMenu : MenuModel
    {
        private CharacterEditorMenu Root;

        private ItemSelector HairModel;
        private ItemSelector PrimaryHairColor;
        private ItemSelector SecondaryHaircolor;

        public MPHairMenu(CharacterEditorMenu root)
        {
            Root = root;

            headerTitle = "Hair";
            statusTitle = "";

            HairModel = new ItemSelector(this, "Hair model", API.GetNumberOfPedDrawableVariations(Game.PlayerPed.Handle, 2));
            PrimaryHairColor = new ItemSelector(this, "Primary hair color", 63);
            SecondaryHaircolor = new ItemSelector(this, "Secondary hair color", 63);

            menuItems.Add(HairModel);
            menuItems.Add(PrimaryHairColor);
            menuItems.Add(SecondaryHaircolor);
            menuItems.Add(new MenuItemStandard { Title = "Back", OnActivate = CloseMenu });
        }

        public void SetNewAppearance()
        {
            API.SetPedComponentVariation(Game.PlayerPed.Handle, (int)PedComponents.Hair, API.GetNumberOfPedTextureVariations(Game.PlayerPed.Handle, Game.Player.Character.Handle, 2), 0, 0);
            API.SetPedHairColor(Game.PlayerPed.Handle, PrimaryHairColor.Value, SecondaryHaircolor.Value);

            Root.AdditionalSaveData.PrimaryHairColor = (byte)PrimaryHairColor.Value;
            Root.AdditionalSaveData.SecondaryHairColor = (byte)SecondaryHaircolor.Value;
        }

        public override void OnTick(long frameCount, int frameTime, long gameTimer)
        {
            base.OnTick(frameCount, frameTime, gameTimer);

            foreach (MenuItem item in menuItems)
            {
                item.OnTick(frameCount, frameTime, gameTimer);
            }

            if (Root.Observer.CurrentMenu == this)
            {
                if (Game.IsDisabledControlJustReleased(0, Control.FrontendCancel))
                {
                    CloseMenu(null);
                }

                SelectedIndex = MathUtil.Clamp(SelectedIndex, 0, menuItems.Count - 1);
            }
        }

        private void CloseMenu(MenuItemStandard m)
        {
            if (Root.Observer.CurrentMenu == this)
            {
                Root.Observer.CloseMenu();
            }
        }
    }
}
