using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.net.Entity;
using MenuAPI;
using System;
using System.Collections.Generic;

namespace Curiosity.Menus.Client.net.Classes.Menus.PlayerCreator
{
    class PlayerReset
    {
        static bool resetBuffer = false;

        public static async void Init()
        {
            while (!PlayerCreatorMenu.MenuSetup)
                await BaseScript.Delay(0);

            PlayerCreatorMenu.menu.AddMenuItem(new MenuItem("Reset Changes") { ItemData = "RESET", Description = "~r~Warning:~s~ Will reset all changes ~r~except for gender~s~ to how you looked when you connected." });
        }

        public static async void ResetCharacter()
        {
            if (resetBuffer)
                return;

            resetBuffer = true;

            User user = Client.User;

            int playerPed = Client.PedHandle;

            API.SetPedHeadBlendData(playerPed, user.Skin.FatherAppearance, user.Skin.MotherAppearance, 0, user.Skin.FatherSkin, user.Skin.MotherSkin, 0, user.Skin.FatherMotherAppearanceGene, user.Skin.FatherMotherSkinGene, 0, false);

            API.SetPedEyeColor(playerPed, user.Skin.EyeColor);

            API.SetPedHairColor(playerPed, user.Skin.HairColor, user.Skin.HairSecondaryColor);

            foreach (KeyValuePair<int, Tuple<int, int>> comp in user.Skin.Components)
            {
                await Client.Delay(0);
                API.SetPedComponentVariation(playerPed, comp.Key, comp.Value.Item1, comp.Value.Item2, 0);
            }

            foreach (KeyValuePair<int, int> over in user.Skin.PedHeadOverlay)
            {
                await Client.Delay(0);
                API.SetPedHeadOverlay(playerPed, over.Key, over.Value, over.Value == 0 ? 0f : 1.0f);
            }

            foreach (KeyValuePair<int, Tuple<int, int>> over in user.Skin.PedHeadOverlayColor)
            {
                await Client.Delay(0);
                API.SetPedHeadOverlayColor(playerPed, over.Key, over.Value.Item1, over.Value.Item2, 0);
            }

            foreach (KeyValuePair<int, Tuple<int, int>> over in user.Skin.Props)
            {
                await Client.Delay(0);
                API.SetPedPropIndex(playerPed, over.Key, over.Value.Item1, over.Value.Item2, false);
            }

            Game.PlayerPed.Weapons.RemoveAll();

            await BaseScript.Delay(2000);

            resetBuffer = false;
        }
    }
}
