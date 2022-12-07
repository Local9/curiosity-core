namespace Curiosity.Framework.Shared.SerializedModels
{
    [Serializable]
    public partial class CameraVector
    {
        [JsonProperty("x")]
        public float X { get; set; }
        
        [JsonProperty("y")]
        public float Y { get; set; }
        
        [JsonProperty("z")]
        public float Z { get; set; }

        public CameraVector() { }

        public override string ToString()
        {
            return $"({X}f, {Y}f, {Z}f)";
        }
    }
}
