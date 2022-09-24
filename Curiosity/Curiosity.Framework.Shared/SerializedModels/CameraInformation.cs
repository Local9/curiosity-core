﻿using FxEvents.Shared.Attributes;

namespace Curiosity.Framework.Shared.SerializedModels
{
    [Serializable]
    public partial class CameraInformation
    {
        [JsonProperty("position")]
        public CameraVector Position { get; set; }
        
        [JsonProperty("rotation")]
        public CameraVector Rotation { get; set; }

        [JsonProperty("direction")]
        public CameraVector Direction { get; set; }

        [JsonProperty("fov")]
        public float FieldOfView { get; set; }

        public CameraInformation() { }

        [Ignore]
        public override string ToString()
        {
            return $"P: {Position}, R: {Rotation}, D: {Direction}, F: {FieldOfView}f";
        }
    }
}
