using NativeUI;
using static Curiosity.Core.Client.Utils.ShopPed;

namespace Curiosity.Core.Client.Interface.Menus.Creator
{
    class CharacterCustomisation
    {
        int componentIndex = 1;

        public UIMenu CreateMenu(UIMenu menu)
        {
            menu.Subtitle.Caption = "Customise Ped Clothing";

            string[] clothingCategoryNames = new string[12] { "Unused (head)", "Masks", "Unused (hair)", "Upper Body", "Lower Body", "Bags & Parachutes", "Shoes", "Scarfs & Chains", "Shirt & Accessory", "Body Armor", "Badges & Logos", "Jackets" };
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
                if (newIndex == 0) // Masks
                    componentIndex = 1;
                if (newIndex == 1) // Upper Body
                    componentIndex = 3;
                if (newIndex == 2) // Lower Body
                    componentIndex = 4;
                if (newIndex == 3) // Bags & Parachutes
                    componentIndex = 5;
                if (newIndex == 4) // Shoes
                    componentIndex = 6;
                if (newIndex == 5) // Scarfs & Chains
                    componentIndex = 7;
                if (newIndex == 6) // Shirt & Accessory
                    componentIndex = 8;
                if (newIndex == 7) // Body Armor & Accessory
                    componentIndex = 9;
                if (newIndex == 8) // Badges & Logos
                    componentIndex = 10;
                if (newIndex == 9) // Shirt Overlay & Jackets
                    componentIndex = 11;
            };

            menu.OnListChange += (_sender, listItem, listIndex) =>
            {
                int textureIndex = GetPedTextureVariation(Cache.PlayerPed.Handle, componentIndex);
                int newTextureIndex = 0;
                SetPedComponentVariation(Cache.PlayerPed.Handle, componentIndex, listIndex, newTextureIndex, 2);

                if (componentIndex == 11)
                {
                    Tuple<int, int> item = GetProperTorso(listIndex, newTextureIndex);
                    Logger.Debug($"GetProperTorso: {item.Item1}/{item.Item2}");
                    if (item.Item1 != -1 && item.Item2 != -1)
                    {
                        SetPedComponentVariation(Cache.PlayerPed.Handle, 3, item.Item1, item.Item2, 2);
                        Cache.Character.CharacterInfo.DrawableVariations[3] = new KeyValuePair<int, int>(item.Item1, item.Item2);
                    }
                }

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

        Tuple<int, int> GetProperTorso(int drawable, int texture)
        {
            int pedHandle = Game.PlayerPed.Handle;

            PedHash pedModel = (PedHash)Game.PlayerPed.Model.Hash;
            if (!pedModel.Equals(PedHash.FreemodeFemale01) && !pedModel.Equals(PedHash.FreemodeMale01))
            {
                return Tuple.Create(-1, -1);
            }

            int topHash = GetHashNameForComponent(pedHandle, 11, drawable, texture);
            int fcDrawable = -1, fcTexture = -1;

            for (int i = 0; i < GetNumForcedComponents((uint)topHash); i++)
            {
                int fcNameHash = -1, fcEnumValue = -1, fcType = -1;
                GetForcedComponent((uint)topHash, i, ref fcNameHash, ref fcEnumValue, ref fcType);

                if (fcType == 3)
                {
                    if (fcNameHash == 0 || fcNameHash == GetHashKey("0"))
                    {
                        fcDrawable = fcEnumValue;
                        fcTexture = 0;
                    }
                    else
                    {
                        PedComponentData data = ShopPed.GetShopPedComponent((uint)fcNameHash);
                        fcDrawable = data.drawable;
                        fcTexture = data.texture;
                    }
                }
            }

            return Tuple.Create(fcDrawable, fcTexture);
        }

        Tuple<int, int> GetProperLegs(int drawable, int texture)
        {
            Tuple<int, int> item = Tuple.Create(-1, -1);
            int pedHandle = Game.PlayerPed.Handle;

            PedHash pedModel = (PedHash)Game.PlayerPed.Model.Hash;
            if (!pedModel.Equals(PedHash.FreemodeFemale01) && !pedModel.Equals(PedHash.FreemodeMale01))
            {
                return item;
            }

            int topHash = GetHashNameForComponent(pedHandle, 4, drawable, texture);
            int fcDrawable = -1, fcTexture = -1;

            for (int i = 0; i < GetNumForcedComponents((uint)topHash); i++)
            {
                int fcNameHash = -1, fcEnumValue = -1, fcType = -1;
                GetForcedComponent((uint)topHash, i, ref fcNameHash, ref fcEnumValue, ref fcType);

                if (fcType == 3)
                {
                    if (fcNameHash == 0 || fcNameHash == GetHashKey("0"))
                    {
                        fcDrawable = fcEnumValue;
                        fcTexture = 0;
                    }
                    else
                    {
                        PedComponentData data = ShopPed.GetShopPedComponent((uint)fcNameHash);
                        fcDrawable = data.drawable;
                        fcTexture = data.texture;
                    }
                }
            }

            item = Tuple.Create(fcDrawable, fcTexture);

            return item;
        }

        void CorrectVariations(int entity)
        {
            int modelHash = GetEntityModel(entity);
            int torsoDrawable = GetPedDrawableVariation(entity, 11);
            int torsoTexture = GetPedTextureVariation(entity, 11);
            int maskDrawable = GetPedDrawableVariation(entity, 1);
            int maskTexture = GetPedTextureVariation(entity, 1);
            int undershirtDrawable = GetPedDrawableVariation(entity, 8);
            int undershirtTexture = GetPedTextureVariation(entity, 8);
            int badgeDrawable = GetPedDrawableVariation(entity, 10);
            int badgeTexture = GetPedTextureVariation(entity, 10);
            int torsoHashName = GetHashNameForComponent(entity, 11, torsoDrawable, torsoTexture);
            int maskHashName = GetHashNameForComponent(entity, 1, maskDrawable, maskTexture);
            int undershirtHashName = GetHashNameForComponent(entity, 8, undershirtDrawable, undershirtTexture);
            int badgeHashName = GetHashNameForComponent(entity, 10, badgeDrawable, badgeTexture);
            int headPropHashName = GetHashNameForProp(entity, 0, GetPedPropIndex(entity, 0), GetPedPropTextureIndex(entity, 0)); // HeadProp
            bool unknown = false;

        }
    }
}
