namespace Curiosity.Framework.Shared.SerializedModels
{
    [Serializable]
    public partial class PropDrawables
    {
        [JsonProperty("hatOrMask")]
        public int HatOrMask;

        [JsonProperty("ears")]
        public int Ears;

        [JsonProperty("glasses")]
        public int Glasses;

        [JsonProperty("unk3")]
        public int Unk_3;

        [JsonProperty("unk4")]
        public int Unk_4;

        [JsonProperty("unk5")]
        public int Unk_5;

        [JsonProperty("watches")]
        public int Watches;

        [JsonProperty("bracelets")]
        public int Bracelets;

        [JsonProperty("unk8")]
        public int Unk_8;

        public PropDrawables()
        {

        }

        public PropDrawables(int hatOrMask, int ears, int glasses, int unk_3, int unk_4, int unk_5, int watches, int bracelets, int unk_8)
        {
            HatOrMask = hatOrMask;
            Ears = ears;
            Glasses = glasses;
            Unk_3 = unk_3;
            Unk_4 = unk_4;
            Unk_5 = unk_5;
            Watches = watches;
            Bracelets = bracelets;
            Unk_8 = unk_8;
        }
    }
}
