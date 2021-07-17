using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Interface.Menus.Creator
{
    class CharacterCustomisation
    {
        private Dictionary<UIMenuListItem, int> drawablesMenuListItems = new Dictionary<UIMenuListItem, int>();
        private Dictionary<UIMenuListItem, int> propsMenuListItems = new Dictionary<UIMenuListItem, int>();

        public UIMenu CreateMenu(UIMenu menu)
        {
            menu.Subtitle.Caption = "Customise Ped";

            menu.OnListChange += Menu_OnListChange;
            menu.OnListSelect += Menu_OnListSelect;
            menu.OnMenuStateChanged += Menu_OnMenuStateChanged;

            return menu;
        }

        private void Menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.Opened || state == MenuState.ChangeForward)
                RefreshMenuItems(newMenu);
        }

        private void RefreshMenuItems(UIMenu menu)
        {
            drawablesMenuListItems.Clear();
            propsMenuListItems.Clear();
            menu.Clear();

            for (int drawable = 0; drawable < 12; drawable++)
            {
                int currentDrawable = GetPedDrawableVariation(Cache.PlayerPed.Handle, drawable);
                int maxVariations = GetNumberOfPedDrawableVariations(Cache.PlayerPed.Handle, drawable);
                int maxTextures = GetNumberOfPedTextureVariations(Cache.PlayerPed.Handle, drawable, currentDrawable);

                if (maxVariations > 0)
                {
                    List<dynamic> drawableTexturesList = new List<dynamic>();

                    for (int i = 0; i < maxVariations; i++)
                    {
                        drawableTexturesList.Add($"Drawable #{i + 1} (of {maxVariations})");
                    }

                    UIMenuListItem drawableTextures = new UIMenuListItem($"{textureNames[drawable]}", drawableTexturesList, currentDrawable, $"Use ← & → to select a ~o~{textureNames[drawable]} Variation~s~, press ~r~enter~s~ to cycle through the available textures.");
                    drawablesMenuListItems.Add(drawableTextures, drawable);
                    menu.AddItem(drawableTextures);
                }
            }

            for (int tmpProp = 0; tmpProp < 5; tmpProp++)
            {
                int realProp = tmpProp > 2 ? tmpProp + 3 : tmpProp;

                int currentProp = GetPedPropIndex(Cache.PlayerPed.Handle, realProp);
                int maxPropVariations = GetNumberOfPedPropDrawableVariations(Cache.PlayerPed.Handle, realProp);

                if (maxPropVariations > 0)
                {
                    List<dynamic> propTexturesList = new List<dynamic>();

                    propTexturesList.Add($"Prop #1 (of {maxPropVariations + 1})");
                    for (int i = 0; i < maxPropVariations; i++)
                    {
                        propTexturesList.Add($"Prop #{i + 2} (of {maxPropVariations + 1})");
                    }


                    UIMenuListItem propTextures = new UIMenuListItem($"{propNames[tmpProp]}", propTexturesList, currentProp + 1, $"Use ← & → to select a ~o~{propNames[tmpProp]} Variation~s~, press ~r~enter~s~ to cycle through the available textures.");
                    propsMenuListItems.Add(propTextures, realProp);
                    menu.AddItem(propTextures);

                }
            }

            menu.RefreshIndex();
        }

        private void Menu_OnListSelect(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (drawablesMenuListItems.ContainsKey(listItem)) // drawable
            {
                int currentDrawableID = drawablesMenuListItems[listItem];
                int currentTextureIndex = GetPedTextureVariation(Cache.PlayerPed.Handle, currentDrawableID);
                int maxDrawableTextures = GetNumberOfPedTextureVariations(Cache.PlayerPed.Handle, currentDrawableID, newIndex) - 1;

                if (currentTextureIndex == -1)
                    currentTextureIndex = 0;

                int newTexture = currentTextureIndex < maxDrawableTextures ? currentTextureIndex + 1 : 0;

                SetPedComponentVariation(Cache.PlayerPed.Handle, currentDrawableID, newIndex, newTexture, 0);
            }
            else if (propsMenuListItems.ContainsKey(listItem)) // prop
            {
                int currentPropIndex = propsMenuListItems[listItem];
                int currentPropVariationIndex = GetPedPropIndex(Cache.PlayerPed.Handle, currentPropIndex);
                int currentPropTextureVariation = GetPedPropTextureIndex(Cache.PlayerPed.Handle, currentPropIndex);
                int maxPropTextureVariations = GetNumberOfPedPropTextureVariations(Cache.PlayerPed.Handle, currentPropIndex, currentPropVariationIndex) - 1;

                int newPropTextureVariationIndex = currentPropTextureVariation < maxPropTextureVariations ? currentPropTextureVariation + 1 : 0;
                SetPedPropIndex(Cache.PlayerPed.Handle, currentPropIndex, currentPropVariationIndex, newPropTextureVariationIndex, true);
            }
        }

        private void Menu_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (drawablesMenuListItems.ContainsKey(listItem))
            {
                int drawableId = drawablesMenuListItems[listItem];
                SetPedComponentVariation(Cache.PlayerPed.Handle, drawableId, newIndex, 0, 0);
                // Need to store the drawableId as a Dictionary with an object.
            }
            else if (propsMenuListItems.ContainsKey(listItem))
            {
                int propId = propsMenuListItems[listItem];
                
                if (newIndex == 0)
                {
                    SetPedPropIndex(Cache.PlayerPed.Handle, propId, newIndex - 1, 0, false);
                    ClearPedProp(Cache.PlayerPed.Handle, propId);
                }
                else
                {
                    SetPedPropIndex(Cache.PlayerPed.Handle, propId, newIndex - 1, 0, true);
                }

                if (propId == 0)
                {
                    int component = GetPedPropIndex(Cache.PlayerPed.Handle, 0); // helm
                    int texture = GetPedPropTextureIndex(Cache.PlayerPed.Handle, 0); // texture
                    int compHash = GetHashNameForProp(Cache.PlayerPed.Handle, 0, component, texture);
                    if (N_0xd40aac51e8e4c663((uint)compHash) > 0) // visor
                    {
                        BeginTextCommandDisplayHelp("TWOSTRINGS");
                        AddTextComponentSubstringPlayerName("Hold ~INPUT_SWITCH_VISOR~ to flip your helmet visor open or closed");
                        AddTextComponentSubstringPlayerName("when on foot or on a motorcycle and when vMenu is closed.");
                        EndTextCommandDisplayHelp(0, false, true, 6000);
                    }
                }
            }
        }

        #region Textures & Props
        private readonly List<string> textureNames = new List<string>()
        {
            "Head",
            "Mask / Facial Hair",
            "Hair Style / Color",
            "Hands / Upper Body",
            "Legs / Pants",
            "Bags / Parachutes",
            "Shoes",
            "Neck / Scarfs",
            "Shirt / Accessory",
            "Body Armor / Accessory 2",
            "Badges / Logos",
            "Shirt Overlay / Jackets",
        };

        private readonly List<string> propNames = new List<string>()
        {
            "Hats / Helmets", // id 0
            "Glasses", // id 1
            "Misc", // id 2
            "Watches", // id 6
            "Bracelets", // id 7
        };
        #endregion
    }
}
