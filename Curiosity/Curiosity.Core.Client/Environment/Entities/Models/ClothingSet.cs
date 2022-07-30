using Curiosity.Systems.Library.Enums;

namespace Curiosity.Core.Client.Environment.Entities.Models
{
    internal class ClothingSet
    {
        public class CS
        {
            public int ComponentID;
            public int DrawableID;
            public int TextureID;
            public int PaletteID;

            public CS(int cid, int did, int tid, int pid = 2)
            {
                ComponentID = cid;
                DrawableID = did;
                TextureID = tid;
                PaletteID = pid;
            }

            public CS()
            {
                ComponentID = -1;
                DrawableID = -1;
                TextureID = -1;
                PaletteID = -1;
            }
        }

        public string Name;
        public string LocalizedName;
        public eClothingType Type;
        public CS Set1, Set2, Set3, Set4, Set5, Set6, Set7, Set8, Set9, Set10, Set11;

        public ClothingSet(string name, string local, CS s1, CS s2, CS s3, CS s4, CS s5, CS s6, CS s7)
        {
            this.Name = name;
            LocalizedName = local;
            this.Type = eClothingType.FullSuit;
            Set1 = s1;
            Set2 = s2;
            Set3 = s3;
            Set4 = s4;
            Set5 = s5;
            Set6 = s6;
            Set7 = s7;
            Set8 = null;
            Set9 = null;
            Set10 = null;
        }

        public ClothingSet(string name, string local, CS s1, CS s2, CS s3, CS s4, CS s5, CS s6, CS s7, CS s8, CS s9, CS s10)
        {
            this.Name = name;
            LocalizedName = local;
            this.Type = eClothingType.Outfit;
            Set1 = s1;
            Set2 = s2;
            Set3 = s3;
            Set4 = s4;
            Set5 = s5;
            Set6 = s6;
            Set7 = s7;
            Set8 = s8;
            Set9 = s9;
            Set10 = s10;
            Set11 = null;
        }

        public ClothingSet(string name, string local, CS s1, CS s2, CS s3, CS s4, CS s5, CS s6, CS s7, CS s8, CS s9, CS s10, CS s11)
        {
            this.Name = name;
            LocalizedName = local;
            this.Type = eClothingType.Outfit;
            Set1 = s1;
            Set2 = s2;
            Set3 = s3;
            Set4 = s4;
            Set5 = s5;
            Set6 = s6;
            Set7 = s7;
            Set8 = s8;
            Set9 = s9;
            Set10 = s10;
            Set11 = s11;
        }

        public string GetLocalizedName()
        {
            string result = null;
            if (Name == "")
                result = LocalizedName;
            else
                result = Game.GetGXTEntry(Name);
            if (result == "NULL")
                return LocalizedName;
            else
                return result;
        }
    }
}
