using CitizenFX.Core.Native;
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

            return menu;
        }

        private void Menu_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (drawablesMenuListItems.ContainsKey(listItem))
            {
                int drawableId = drawablesMenuListItems[listItem];
                API.SetPedComponentVariation(Cache.PlayerPed.Handle, drawableId, newIndex, 0, 0);
                // Need to store the drawableId as a Dictionary with an object.
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
