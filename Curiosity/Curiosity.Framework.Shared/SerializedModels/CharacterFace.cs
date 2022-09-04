namespace Curiosity.Framework.Shared.SerializedModels
{
    [Serializable]
    public partial class Face
    {
        [JsonProperty("mother")]
        public int Mother { get; internal set; }

        [JsonProperty("father")]
        public int Father { get; internal set; }

        [JsonProperty("resemblance")]
        public float Resemblance { get; internal set; }

        [JsonProperty("skinBlend")]
        public float SkinBlend { get; internal set; }

        [JsonProperty("features")]
        public float[] Features { get; internal set; }

        [JsonProperty("beard")]
        public A3 Beard = new A3();

        [JsonProperty("blemishes")]
        public A2 Blemishes = new A2();

        [JsonProperty("eyebrow")]
        public A3 Eyebrow = new A3();

        [JsonProperty("makeup")]
        public A2 Makeup = new A2();

        [JsonProperty("blusher")]
        public A3 Blusher = new A3();

        [JsonProperty("complexion")]
        public A2 Complexion = new A2();

        [JsonProperty("skinDamage")]
        public A2 SkinDamage = new A2();

        [JsonProperty("lipstick")]
        public A3 Lipstick = new A3();

        [JsonProperty("freckles")]
        public A2 Freckles = new A2();

        [JsonProperty("eye")]
        public Eye Eye = new Eye();
    }
}
