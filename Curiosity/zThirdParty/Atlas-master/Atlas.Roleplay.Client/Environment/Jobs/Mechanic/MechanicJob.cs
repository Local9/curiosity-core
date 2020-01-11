using System.Collections.Generic;
using Atlas.Roleplay.Client.Environment.Jobs.Profiles;
using Atlas.Roleplay.Library.Models;

namespace Atlas.Roleplay.Client.Environment.Jobs.Mechanic
{
    public class MechanicJob : Job
    {
        public override Employment Attachment { get; set; }
        public override string Label { get; set; }
        public override BlipInfo[] Blips { get; set; }
        public override Dictionary<int, string> Roles { get; set; }

        public override JobProfile[] Profiles { get; set; } =
        {
            new JobStorageProfile
            {
                Position = new Position(-342.0678f, -122.9054f, 39.00961f, 346.1445f)
            }
        };
    }
}