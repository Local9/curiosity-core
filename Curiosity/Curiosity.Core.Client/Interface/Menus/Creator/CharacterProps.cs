using NativeUI;
using System.Collections.Generic;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Interface.Menus.Creator
{
    class CharacterProps
    {
        int propIndex = 0;

        public UIMenu CreateMenu(UIMenu menu)
        {
            string[] propNames = new string[5] { "Hats & Helmets", "Glasses", "Misc Props", "Watches", "Bracelets" };
            for (int x = 0; x < 5; x++)
            {
                int propId = x;
                if (x > 2)
                {
                    propId += 3;
                }

                int currentProp = Cache.Character.CharacterInfo.Props.ContainsKey(propId) ? Cache.Character.CharacterInfo.Props[propId].Key : GetPedPropIndex(Cache.PlayerPed.Handle, propId);
                int currentPropTexture = Cache.Character.CharacterInfo.Props.ContainsKey(propId) ? Cache.Character.CharacterInfo.Props[propId].Value : GetPedPropTextureIndex(Cache.PlayerPed.Handle, propId);

                List<dynamic> propsList = new List<dynamic>();
                for (int i = 0; i < GetNumberOfPedPropDrawableVariations(Cache.PlayerPed.Handle, propId); i++)
                {
                    propsList.Add($"Prop #{i} (of {GetNumberOfPedPropDrawableVariations(Cache.PlayerPed.Handle, propId)})");
                }
                propsList.Add("No Prop");


                if (GetPedPropIndex(Cache.PlayerPed.Handle, propId) != -1)
                {
                    int maxPropTextures = GetNumberOfPedPropTextureVariations(Cache.PlayerPed.Handle, propId, currentProp);
                    UIMenuListItem propListItem = new UIMenuListItem($"{propNames[x]}", propsList, currentProp, $"Select a prop using the arrow keys and press ~o~enter~s~ to cycle through all available textures. Currently selected texture: #{currentPropTexture + 1} (of {maxPropTextures}).");
                    menu.AddItem(propListItem);
                }
                else
                {
                    UIMenuListItem propListItem = new UIMenuListItem($"{propNames[x]}", propsList, currentProp, "Select a prop using the arrow keys and press ~o~enter~s~ to cycle through all available textures.");
                    menu.AddItem(propListItem);
                }


            }

            menu.OnIndexChange += (_sender, newIndex) =>
            {
                if (newIndex == 0)
                    propIndex = 0;
                if (newIndex == 1)
                    propIndex = 1;
                if (newIndex == 2)
                    propIndex = 2;
                if (newIndex == 3)
                    propIndex = 6;
                if (newIndex == 4)
                    propIndex = 7;
            };

            menu.OnListChange += (_sender, listItem, listIndex) =>
            {
                int textureIndex = 0;
                if (listIndex >= GetNumberOfPedPropDrawableVariations(Cache.PlayerPed.Handle, propIndex))
                {
                    SetPedPropIndex(Cache.PlayerPed.Handle, propIndex, -1, -1, false);
                    ClearPedProp(Cache.PlayerPed.Handle, propIndex);
                    if (Cache.Character.CharacterInfo.Props == null)
                    {
                        Cache.Character.CharacterInfo.Props = new Dictionary<int, KeyValuePair<int, int>>();
                    }
                    Cache.Character.CharacterInfo.Props[propIndex] = new KeyValuePair<int, int>(-1, -1);
                    listItem.Description = $"Select a prop using the arrow keys and press ~o~enter~s~ to cycle through all available textures.";
                }
                else
                {
                    SetPedPropIndex(Cache.PlayerPed.Handle, propIndex, listIndex, textureIndex, true);
                    if (Cache.Character.CharacterInfo.Props == null)
                    {
                        Cache.Character.CharacterInfo.Props = new Dictionary<int, KeyValuePair<int, int>>();
                    }
                    Cache.Character.CharacterInfo.Props[propIndex] = new KeyValuePair<int, int>(listIndex, textureIndex);
                    if (GetPedPropIndex(Cache.PlayerPed.Handle, propIndex) == -1)
                    {
                        listItem.Description = $"Select a prop using the arrow keys and press ~o~enter~s~ to cycle through all available textures.";
                    }
                    else
                    {
                        int maxPropTextures = GetNumberOfPedPropTextureVariations(Cache.PlayerPed.Handle, propIndex, listIndex);
                        listItem.Description = $"Select a prop using the arrow keys and press ~o~enter~s~ to cycle through all available textures. Currently selected texture: #{textureIndex + 1} (of {maxPropTextures}).";
                    }
                }
                _sender.UpdateDescription();
            };

            menu.OnListSelect += (_sender, listItem, listIndex) =>
            {
                int textureIndex = GetPedPropTextureIndex(Cache.PlayerPed.Handle, propIndex);
                int newTextureIndex = (GetNumberOfPedPropTextureVariations(Cache.PlayerPed.Handle, propIndex, listIndex) - 1) < (textureIndex + 1) ? 0 : textureIndex + 1;
                if (textureIndex >= GetNumberOfPedPropDrawableVariations(Cache.PlayerPed.Handle, propIndex))
                {
                    SetPedPropIndex(Cache.PlayerPed.Handle, propIndex, -1, -1, false);
                    ClearPedProp(Cache.PlayerPed.Handle, propIndex);
                    if (Cache.Character.CharacterInfo.Props == null)
                    {
                        Cache.Character.CharacterInfo.Props = new Dictionary<int, KeyValuePair<int, int>>();
                    }
                    Cache.Character.CharacterInfo.Props[propIndex] = new KeyValuePair<int, int>(-1, -1);
                    listItem.Description = $"Select a prop using the arrow keys and press ~o~enter~s~ to cycle through all available textures.";
                }
                else
                {
                    SetPedPropIndex(Cache.PlayerPed.Handle, propIndex, listIndex, newTextureIndex, true);
                    if (Cache.Character.CharacterInfo.Props == null)
                    {
                        Cache.Character.CharacterInfo.Props = new Dictionary<int, KeyValuePair<int, int>>();
                    }
                    Cache.Character.CharacterInfo.Props[propIndex] = new KeyValuePair<int, int>(listIndex, newTextureIndex);
                    if (GetPedPropIndex(Cache.PlayerPed.Handle, propIndex) == -1)
                    {
                        listItem.Description = $"Select a prop using the arrow keys and press ~o~enter~s~ to cycle through all available textures.";
                    }
                    else
                    {
                        int maxPropTextures = GetNumberOfPedPropTextureVariations(Cache.PlayerPed.Handle, propIndex, listIndex);
                        listItem.Description = $"Select a prop using the arrow keys and press ~o~enter~s~ to cycle through all available textures. Currently selected texture: #{newTextureIndex + 1} (of {maxPropTextures}).";
                    }
                }
                _sender.UpdateDescription();
            };

            return menu;
        }
    }
}
