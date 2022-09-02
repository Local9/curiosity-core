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
    }
}
