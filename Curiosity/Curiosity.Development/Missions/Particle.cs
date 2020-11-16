﻿using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Enums;
using System.Threading.Tasks;

namespace Curiosity.Development.Missions
{
    [MissionInfo("Development Particle", "devPart", 0f, 0f, 0f, MissionType.Mission, true, "None")]
    public class Particle : Mission
    {
        public async override void Start()
        {
            Mission.CreateParticleAtLocation("core", "exp_grd_flare", Players[0].Character.Position.Around(1f, 1f));

            MissionManager.Instance.RegisterTickHandler(OnMissionTick);
        }

        public override void End()
        {
            MissionManager.Instance.DeregisterTickHandler(OnMissionTick);
        }

        async Task OnMissionTick()
        {

        }
    }
}
