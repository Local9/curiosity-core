﻿using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Curiosity.Core.Server.Environment
{
    public class Tip
    {
        [DataMember(Name = "header")]
        public string Header { get; set; }

        [DataMember(Name = "content")]
        public string Content { get; set; }
    }

    public class ServerConfig
    {
        [DataMember(Name = "tips")]
        public List<Tip> Tips { get; set; }

        [DataMember(Name = "loadingImages")]
        public List<string> LoadingImages { get; set; }

        [DataMember(Name = "policeSpeedLimitWarning")]
        public float PoliceSpeedLimitWarning;
    }
}
