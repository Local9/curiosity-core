namespace Curiosity.Framework.Shared.SerializedModels
{
    [Serializable]
    public partial class ComponentDrawables
    {
        [JsonProperty("face")]
        public int Face;

        [JsonProperty("mask")]
        public int Mask;

        [JsonProperty("hair")]
        public int Hair;

        [JsonProperty("torso")]
        public int Torso;

        [JsonProperty("leg")]
        public int Leg;

        [JsonProperty("bagOrParachute")]
        public int BagOrParachute;

        [JsonProperty("shoes")]
        public int Shoes;

        [JsonProperty("accessory")]
        public int Accessory;

        [JsonProperty("undershirt")]
        public int Undershirt;

        [JsonProperty("kevlar")]
        public int Kevlar;

        [JsonProperty("badge")]
        public int Badge;

        [JsonProperty("torso_2")]
        public int Torso_2;

        public ComponentDrawables()
        {

        }

        public ComponentDrawables(int face, int mask, int hair, int torso, int leg, int bagOrParachute, int shoes, int accessory, int undershirt, int kevlar, int badge, int torso_2)
        {
            Face = face;
            Mask = mask;
            Hair = hair;
            Torso = torso;
            Leg = leg;
            BagOrParachute = bagOrParachute;
            Shoes = shoes;
            Accessory = accessory;
            Undershirt = undershirt;
            Kevlar = kevlar;
            Badge = badge;
            Torso_2 = torso_2;
        }
    }
}
