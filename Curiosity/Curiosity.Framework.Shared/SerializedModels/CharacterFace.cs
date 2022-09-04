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
        public A3 Beard;

        [JsonProperty("blemishes")]
        public A2 Blemishes;

        [JsonProperty("eyebrow")]
        public A3 Eyebrow;

        [JsonProperty("makeup")]
        public A2 Makeup;

        [JsonProperty("blusher")]
        public A3 Blusher;

        [JsonProperty("complexion")]
        public A2 Complexion;

        [JsonProperty("skinDamage")]
        public A2 SkinDamage;

        [JsonProperty("lipstick")]
        public A3 Lipstick;

        [JsonProperty("freckles")]
        public A2 Freckles;

        [JsonProperty("eye")]
        public Eye Eye;
    }
}
