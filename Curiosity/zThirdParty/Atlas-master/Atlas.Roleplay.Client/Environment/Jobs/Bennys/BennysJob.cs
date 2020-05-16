using Atlas.Roleplay.Library.Models;
using CitizenFX.Core.Native;
using System.Collections.Generic;

namespace Atlas.Roleplay.Client.Environment.Jobs.Bennys
{
    public class BennysJob : Job
    {
        public override Employment Attachment { get; set; }
        public override string Label { get; set; }
        public override BlipInfo[] Blips { get; set; } = { };
        public override Dictionary<int, string> Roles { get; set; }
        public override JobProfile[] Profiles { get; set; } = { };

        public override void Begin()
        {
            API.FreezeEntityPosition(API.GetClosestObjectOfType(-205.7837f, -1310.172f, 31.2959f, 3f, unchecked((uint)-427498890), false, false, false), true);
        }
    }
}