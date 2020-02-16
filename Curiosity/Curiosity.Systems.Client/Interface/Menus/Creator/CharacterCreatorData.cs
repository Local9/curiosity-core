using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Interface.Menus.Creator
{
    class CharacterCreatorData
    {
        /// <summary>
        /// Hair - MF
        /// Eyebrows - MF
        /// Facial Hair - M
        /// Skin Blemishes - MF
        /// Skin Aging - MF
        /// Skin Complexion - MF
        /// Moles & Freckles - MF
        /// Skin Damage - MF
        /// Eye Color - MF
        /// Eye Makeup - MF
        /// Blusher - F
        /// Lipstick - MF
        /// </summary>


        public Dictionary<string, int> Hair = new Dictionary<string, int>() {
            { "long", 1 },
            { "short", 2 },
        };

        public Dictionary<string, int> Eyebrows = new Dictionary<string, int>();
        public Dictionary<string, int> SkinBlemishes = new Dictionary<string, int>();
        public Dictionary<string, int> SkinAging = new Dictionary<string, int>();
        public Dictionary<string, int> SkinComplexion = new Dictionary<string, int>();
        public Dictionary<string, int> SkinMoles = new Dictionary<string, int>();
        public Dictionary<string, int> SkinDamage = new Dictionary<string, int>();
        public Dictionary<string, int> EyeColor = new Dictionary<string, int>();
        
        public Dictionary<string, int> EyeMakeup = new Dictionary<string, int>();
        public Dictionary<string, int> Blusher = new Dictionary<string, int>();
        public Dictionary<string, int> Lipstick = new Dictionary<string, int>();
        
        public Dictionary<string, int> FacialHair = new Dictionary<string, int>();
    }
}
