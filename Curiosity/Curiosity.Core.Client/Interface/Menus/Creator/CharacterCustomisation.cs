using NativeUI;
using System.Collections.Generic;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Interface.Menus.Creator
{
    class CharacterCustomisation
    {
        int componentIndex = 1;

        public UIMenu CreateMenu(UIMenu menu)
        {
            menu.Subtitle.Caption = "Customise Ped Clothing";

            string[] clothingCategoryNames = new string[12] { "Unused (head)", "Masks", "Unused (hair)", "Upper Body", "Lower Body", "Bags & Parachutes", "Shoes", "Scarfs & Chains", "Shirt & Accessory", "Body Armor & Accessory 2", "Badges & Logos", "Shirt Overlay & Jackets" };
            for (int i = 0; i < 12; i++)
            {
                if (i != 0 && i != 2)
                {
                    int currentVariationIndex = Cache.Character.CharacterInfo.DrawableVariations.ContainsKey(i) ? Cache.Character.CharacterInfo.DrawableVariations[i].Key : GetPedDrawableVariation(Cache.PlayerPed.Handle, i);
                    int currentVariationTextureIndex = Cache.Character.CharacterInfo.DrawableVariations.ContainsKey(i) ? Cache.Character.CharacterInfo.DrawableVariations[i].Value : GetPedTextureVariation(Cache.PlayerPed.Handle, i);

                    int maxDrawables = GetNumberOfPedDrawableVariations(Cache.PlayerPed.Handle, i);

                    List<dynamic> items = new List<dynamic>();
                    for (int x = 0; x < maxDrawables; x++)
                    {
                        items.Add($"Drawable #{x} (of {maxDrawables})");
                    }

                    int maxTextures = GetNumberOfPedTextureVariations(Cache.PlayerPed.Handle, i, currentVariationIndex);

                    UIMenuListItem listItem = new UIMenuListItem(clothingCategoryNames[i], items, currentVariationIndex, $"Select a drawable using the arrow keys and press ~o~enter~s~ to cycle through all available textures. Currently selected texture: #{currentVariationTextureIndex + 1} (of {maxTextures}).");
                    menu.AddItem(listItem);
                }
            }

            menu.OnIndexChange += (_sender, newIndex) =>
            {
                if (newIndex == 0)
                    componentIndex = 1;
                if (newIndex == 1)
                    componentIndex = 3;
                if (newIndex == 2)
                    componentIndex = 4;
                if (newIndex == 3)
                    componentIndex = 5;
                if (newIndex == 4)
                    componentIndex = 6;
                if (newIndex == 5)
                    componentIndex = 7;
                if (newIndex == 6)
                    componentIndex = 8;
                if (newIndex == 7)
                    componentIndex = 9;
                if (newIndex == 8)
                    componentIndex = 10;
                if (newIndex == 9)
                    componentIndex = 11;
            };

            menu.OnListChange += (_sender, listItem, listIndex) =>
            {
                int textureIndex = GetPedTextureVariation(Cache.PlayerPed.Handle, componentIndex);
                int newTextureIndex = 0;
                SetPedComponentVariation(Cache.PlayerPed.Handle, componentIndex, listIndex, newTextureIndex, 0);
                if (Cache.Character.CharacterInfo.DrawableVariations == null)
                {
                    Cache.Character.CharacterInfo.DrawableVariations = new Dictionary<int, KeyValuePair<int, int>>();
                }

                int maxTextures = GetNumberOfPedTextureVariations(Cache.PlayerPed.Handle, componentIndex, listIndex);

                Cache.Character.CharacterInfo.DrawableVariations[componentIndex] = new KeyValuePair<int, int>(listIndex, newTextureIndex);
                listItem.Description = $"Select a drawable using the arrow keys and press ~o~enter~s~ to cycle through all available textures. Currently selected texture: #{newTextureIndex + 1} (of {maxTextures}).";
                _sender.UpdateDescription();
            };

            menu.OnListSelect += (_sender, listItem, listIndex) =>
            {
                int textureIndex = GetPedTextureVariation(Cache.PlayerPed.Handle, componentIndex);
                int newTextureIndex = (GetNumberOfPedTextureVariations(Cache.PlayerPed.Handle, componentIndex, listIndex) - 1) < (textureIndex + 1) ? 0 : textureIndex + 1;
                SetPedComponentVariation(Cache.PlayerPed.Handle, componentIndex, listIndex, newTextureIndex, 0);
                if (Cache.Character.CharacterInfo.DrawableVariations == null)
                {
                    Cache.Character.CharacterInfo.DrawableVariations = new Dictionary<int, KeyValuePair<int, int>>();
                }

                int maxTextures = GetNumberOfPedTextureVariations(Cache.PlayerPed.Handle, componentIndex, listIndex);

                Cache.Character.CharacterInfo.DrawableVariations[componentIndex] = new KeyValuePair<int, int>(listIndex, newTextureIndex);
                listItem.Description = $"Select a drawable using the arrow keys and press ~o~enter~s~ to cycle through all available textures. Currently selected texture: #{newTextureIndex + 1} (of {maxTextures}).";
                _sender.UpdateDescription();
            };

            return menu;
        }
    }
}
